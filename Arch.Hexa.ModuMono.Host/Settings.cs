using Arch.Hexa.ModuMono.BuildingBlocks.Application.Settings;

namespace Arch.Hexa.ModuMono.Host;

public class Settings(IConfiguration configuration) : ISettings
{
    public string ServiceName { get; } = 
        Environment.GetEnvironmentVariable("SERVICE_NAME") ?? configuration.GetSection("ServiceName").Value ?? "Architecture.Pocs.ModularMonolith";
    public string DatabaseConnectionString { get; } = 
        Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ?? configuration.GetSection("ConnectionStrings:DefaultConnection").Value ?? "Server=(localdb)\\MSSQLLocalDB;Initial Catalog=CommonEntityService;Integrated Security=true";
    public string CorsAllowedOrigins { get; } = 
        Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS") ?? configuration.GetSection("Cors:AllowedOrigins").Value ?? "http://localhost:3000;http://localhost:3001";
    public bool CustomLoggingConsoleEnabled { get; } = 
        Convert.ToBoolean(Environment.GetEnvironmentVariable("LOGGING_CONSOLE_ENABLED") ?? configuration.GetSection("CustomLogging").GetSection("Console:Enabled").Value ?? "true");
    public int LoggingConsoleLevel { get; } = 
        Convert.ToInt32(Environment.GetEnvironmentVariable("LOGGING_CONSOLE_LEVEL") ?? configuration.GetSection("CustomLogging").GetSection("Console:LogLevel").Value ?? "0");
    public bool CustomLoggingSeqEnabled { get; } = 
        Convert.ToBoolean(Environment.GetEnvironmentVariable("LOGGING_SEQ_ENABLED") ?? configuration.GetSection("CustomLogging").GetSection("Seq:Enabled").Value ?? "true");
    public int LoggingSeqLevel { get; } = 
        Convert.ToInt32(Environment.GetEnvironmentVariable("LOGGING_SEQ_LEVEL") ?? configuration.GetSection("CustomLogging").GetSection("Seq:LogLevel").Value ?? "0");
    public string SeqServerUrl { get; } =
        Environment.GetEnvironmentVariable("SEQ_SERVER_URL") ?? configuration.GetSection("CustomLogging").GetSection("Seq:ServerUrl").Value ?? "http://localhost:5341";
    public bool CustomLoggingFileEnabled { get; } = 
        Convert.ToBoolean(Environment.GetEnvironmentVariable("LOGGING_FILE_ENABLED") ?? configuration.GetSection("CustomLogging").GetSection("File:Enabled").Value ?? "true");
    public int LoggingFileLevel { get; } = 
        Convert.ToInt32(Environment.GetEnvironmentVariable("LOGGING_FILE_LEVEL") ?? configuration.GetSection("CustomLogging").GetSection("File:LogLevel").Value ?? "0");
    public string CustomLoggingFilePathFormat { get; } = 
        Environment.GetEnvironmentVariable("LOGGING_FILE_PATH_FORMAT") ?? configuration.GetSection("CustomLogging").GetSection("File:PathFormat").Value ?? "logs\\application-log.log";
    public string TelemetryServerUrl { get; } =
        Environment.GetEnvironmentVariable("TELEMETRY_SERVER_URL") ?? configuration.GetSection("Telemetry").GetSection("ServerUrl").Value ?? "http://localhost:4317";
}

