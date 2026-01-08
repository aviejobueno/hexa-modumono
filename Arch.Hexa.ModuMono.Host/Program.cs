using Arch.Hexa.ModuMono.BuildingBlocks.Api.Handlers;
using Arch.Hexa.ModuMono.BuildingBlocks.Api.Helpers;
using Arch.Hexa.ModuMono.BuildingBlocks.Api.Security.Authorization;
using Arch.Hexa.ModuMono.BuildingBlocks.Application.Converters;
using Arch.Hexa.ModuMono.Customers.RestApi.Contracts.Requests.Customer;
using Arch.Hexa.ModuMono.Customers.RestApi.Contracts.Validators;
using Arch.Hexa.ModuMono.Customers.RestApi.Security;
using Arch.Hexa.ModuMono.Modules.Customers.Application;
using Arch.Hexa.ModuMono.Modules.Customers.Infrastructure;
using Arch.Hexa.ModuMono.Modules.Orders.Application;
using Arch.Hexa.ModuMono.Modules.Orders.Infrastructure;
using Arch.Hexa.ModuMono.Orders.RestApi.Contracts.Requests.Order;
using Arch.Hexa.ModuMono.Orders.RestApi.Contracts.Validators;
using Arch.Hexa.ModuMono.Orders.RestApi.Security;
using DotNetEnv;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Newtonsoft.Json.Linq;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Sinks.OpenTelemetry;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Arch.Hexa.ModuMono.Host;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

//load environment variables
Env.Load();

//set up configuration
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables(prefix: "ASPNETCORE_")
    .AddEnvironmentVariables(prefix: "API_")
    .Build();

//creating the settings
var settings = new Settings(builder.Configuration);

// set up the logger
builder.Logging.ClearProviders();

const string loggerOutputTemplate =
    "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{MachineName}] [{EnvironmentUserName}] [{ThreadId}] [{ThreadName}] [{CorrelationId}] [{TraceId}] [{SpanId}] [{Level}] [{SourceContext}] [{EventId}] {Message}{NewLine}{Exception}";

builder.Host.UseSerilog((ctx, services, loggerConfiguration) =>
{
    loggerConfiguration
        .MinimumLevel.Verbose()
        // Reduce noise from framework
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        // Specific: Kestrel HTTP/2 frames are extremely chatty
        .MinimumLevel.Override("Microsoft.AspNetCore.Server.Kestrel", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Server.Kestrel.Http2", Serilog.Events.LogEventLevel.Warning)
        // Optional: other common noisy categories
        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
        .Destructure.AsScalar<JObject>()
        .Destructure.AsScalar<JArray>()
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentUserName()
        .Enrich.WithThreadId()
        .Enrich.WithThreadName()
        .Enrich.WithCorrelationId()
        .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
        .Enrich.WithSpan();

    if (settings.CustomLoggingConsoleEnabled)
    {
        var consoleLevel = LoggerHelper.GetLoggingLevel(settings.LoggingConsoleLevel);
        loggerConfiguration.WriteTo.Console(
            restrictedToMinimumLevel: consoleLevel,
            outputTemplate: loggerOutputTemplate);
    }

    if (settings.CustomLoggingSeqEnabled)
    {
        var seqLevel = LoggerHelper.GetLoggingLevel(settings.LoggingSeqLevel);
        loggerConfiguration.WriteTo.Seq(
            serverUrl: settings.SeqServerUrl,
            restrictedToMinimumLevel: seqLevel,
            eventBodyLimitBytes: 1048576); // (1048576=1MB), default is 262144=256KB 
    }

    if (settings.CustomLoggingFileEnabled)
    {
        var fileLevel = LoggerHelper.GetLoggingLevel(settings.LoggingFileLevel);
        loggerConfiguration.WriteTo.File(
            path: settings.CustomLoggingFilePathFormat,
            outputTemplate: loggerOutputTemplate,
            restrictedToMinimumLevel: fileLevel,
            rollingInterval: RollingInterval.Day);
    }

    // Send Serilog events via OTLP. (keep Seq / File / Console too)
    loggerConfiguration.WriteTo.OpenTelemetry(otlp =>
     {
         otlp.Endpoint = settings.TelemetryServerUrl;
         otlp.Protocol = OtlpProtocol.Grpc;
     });
});

// set up telemetry
var serviceName = settings.ServiceName;
var serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion, serviceInstanceId: Environment.MachineName)
        .AddAttributes([
            new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName),
        ]))
    .WithTracing(tracing => tracing
        .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(0.10)))
        .AddAspNetCoreInstrumentation(o => o.RecordException = true)
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(settings.TelemetryServerUrl)))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(settings.TelemetryServerUrl)));

/*
Nota sobre logs y telemetría.

Con la actual configuración:
los logs van a Console/Seq/File y OTLP (Serilog sink)
las Traces/Metrics van a OTLP (OTel exporter)

Si en algún momento añadimos: builder.Logging.AddOpenTelemetry(...),
entonces sí tendríamos logs duplicados (porque ILogger<T> se iría a Serilog y también al provider OTel),
por ese motivo con la actual configuración no debemos incluir la linea de código: builder.Logging.AddOpenTelemetry(...)
*/

//Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

//Localization
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new List<CultureInfo> { new("es-ES"), new("en-GB") };
    options.DefaultRequestCulture = new RequestCulture(culture: "es-ES", uiCulture: "es-ES");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    
    // If you read culture from headers, keep only what you need (e.g. Accept-Language)
    options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
});

//Controllers and Json configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateTimeFormatConverter("yyyy-MM-dd HH:mm:ss"));
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

//OpenAPI
builder.Services.AddOpenApi();

//Authentication & Authorization with Entra Azure AD
if (builder.Environment.IsDevelopment())
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration.GetSection("DevJwt:Issuer").Value,
                ValidateAudience = true,
                ValidAudience = builder.Configuration.GetSection("DevJwt:Audience").Value,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("DevJwt:SigningKey").Value!)),
                RoleClaimType = "roles"
            };
        });

    builder.Services.AddAuthorization(o => o.FallbackPolicy = o.DefaultPolicy);
}
else
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = options.DefaultPolicy;
    });
}
builder.Services.AddAuthorizationModules(
    typeof(CustomersAuthorizationModule).Assembly,
    typeof(OrdersAuthorizationModule).Assembly
);

// Register modules

//Customers
builder.Services.AddCustomersApplication();
builder.Services.AddCustomersInfrastructure(settings.DatabaseConnectionString);
//Orders
builder.Services.AddOrdersApplication();
builder.Services.AddOrdersInfrastructure(settings.DatabaseConnectionString);

// controllers validators
builder.Services.AddTransient<IValidator<CreateCustomerRequest>, CreateCustomerRequestValidator>();
builder.Services.AddTransient<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();

// Problem Details & Exception Handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Arch.Hexa.ModuMono.Modules.Customers.Application.AssemblyMarker).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Arch.Hexa.ModuMono.Modules.Orders.Application.AssemblyMarker).Assembly);
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "corsAllowedSpecificOriginsPolicy", policy =>
    {
        policy
            .WithOrigins(settings.CorsAllowedOrigins.Split(";"))
            .WithMethods("GET")
            .AllowAnyHeader();
    });
});

var app = builder.Build();

//logger
app.UseSerilogRequestLogging();

// Culture from the HttpRequest
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("corsAllowedSpecificOriginsPolicy");
app.UseHttpsRedirection();
app.MapControllers();
app.UseExceptionHandler();

app.Run();


