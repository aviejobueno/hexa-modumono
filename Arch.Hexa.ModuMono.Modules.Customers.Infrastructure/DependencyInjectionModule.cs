using Arch.Hexa.ModuMono.BuildingBlocks.Application.Abstractions;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Abstractions;
using Arch.Hexa.ModuMono.Modules.Customers.Domain;
using Arch.Hexa.ModuMono.Modules.Customers.Infrastructure.Persistence;
using Arch.Hexa.ModuMono.Modules.Customers.Infrastructure.Persistence.Repositories;
using Arch.Hexa.ModuMono.Modules.Customers.Infrastructure.Sql.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arch.Hexa.ModuMono.Modules.Customers.Infrastructure
{
    public static class DependencyInjectionModule
    {
        public static IServiceCollection AddCustomersInfrastructure(this IServiceCollection services, string databaseConnectionString)
        {
            // SQL Interceptors
            services.AddSingleton<CustomerWriteSqlInterceptor>();
            services.AddSingleton<CustomerReadSqlInterceptor>();

            // Write DbContext: scoped + tracking
            // Important: make optionsLifetime Singleton to avoid root/scoped mismatch with pooled factory
            services.AddDbContext<CustomerDbContext>((sp, options) =>
                {
                    options.UseSqlServer(databaseConnectionString);
                    options.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());
                    options.AddInterceptors(sp.GetRequiredService<CustomerWriteSqlInterceptor>());
                },
                contextLifetime: ServiceLifetime.Scoped,
                optionsLifetime: ServiceLifetime.Singleton);

            // Read DbContext: pooled factory (short-lived contexts created on demand)
            services.AddPooledDbContextFactory<CustomerDbContext>((sp, options) =>
            {
                options.UseSqlServer(
                    databaseConnectionString,
                    sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                options.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());
                options.AddInterceptors(sp.GetRequiredService<CustomerReadSqlInterceptor>());
            });

            // Repositories and unit of work
            services.AddScoped(typeof(ICustomerWriteRepository<>), typeof(CustomerWriteRepository<>));
            services.AddScoped(typeof(ICustomerReadRepository<,>), typeof(CustomerReadRepository<,>));
            services.AddScoped<ICustomerUnitOfWork, CustomerUnitOfWork>();
            services.AddScoped<ICustomerExistenceChecker>(sp => (ICustomerExistenceChecker)sp.GetRequiredService<ICustomerReadRepository<Customer, Guid>>());

            return services;
        }

        /* Información sobre el uso de AddPooledDbContextFactory y las opciones usadas

        Usar AddPooledDbContextFactory<AppDbContext>() te proporciona una factoría que crea instancias de DbContext 
        a partir de un pool interno, en lugar de depender de un único DbContext scoped por petición.
        Las ventajas clave en tu código son:

        Mayor rendimiento / menos asignaciones
        Crear un DbContext no es especialmente caro, pero bajo carga se nota.
        El pooling reutiliza instancias, reduciendo la presión del GC y mejorando la latencia.

        Más seguro para trabajos en segundo plano / en paralelo
        Un DbContext scoped no es thread-safe y está ligado a un ámbito de DI.
        Con una factoría puedes crear un contexto nuevo por operación 
        (por ejemplo, en hosted services, tareas en paralelo, message handlers) sin complicaciones de scopes.

        Gestión de ciclo de vida más limpia
        La aplicación “pides” explícitamente un contexto y have el "dispose" cuando termina. 
        Eso dificulta que, por accidente, mantenga un contexto vivo demasiado tiempo o lo comparta entre capas/hilos.

        Muy buen encaje para GraphQL / resolvers o patrones de alta dispersión de llamadas.
        En GraphQL (o en cualquier escenario donde “una petición dispara muchas consultas pequeñas”), 
        una factoría con pool ayuda a reducir el overhead cuando muchos resolvers crean contextos.

        Configuración consistente por cada contexto creado
        Cada contexto creado por la factoría está garantizado que use las mismas opciones:
            UseSqlServer(settings.DatabaseConnectionString, …)
            UseQuerySplittingBehavior(SplitQuery)
            UseLoggerFactory(...)




        Qué te aporta cada opción de tu código:

        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery):
        Ayuda a evitar la “explosión cartesiana” cuando EF genera JOINs grandes para múltiples Include/colecciones.
        A menudo mejora el rendimiento y el uso de memoria en grafos de objetos complejos, 
        a costa de ejecutar varias consultas SQL(vigila los viajes extra a la base de datos).

        .UseLoggerFactory(service.GetRequiredService<ILoggerFactory>()):
        Asegura que los logs de EF pasen por el pipeline de logging de tu aplicación(Serilog/MEL), 
        con formato consistente, IDs de correlación, etc.

        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        Es “mejor” si la gran mayoría de tus operaciones son lecturas y no actualizas entidades cargadas por ese mismo contexto.

        Cuándo Sí suele ser buena idea: 
            (especialmente en APIs/GraphQL)
            Lecturas predominantes(listados, queries, proyecciones).
            Quieres mejor rendimiento y menos memoria(no se crea el change tracker por cada entidad).
            En GraphQL es muy típico: muchas resoluciones “solo lectura”.

        Cuándo No es buena idea:
        (o te puede dar sorpresas)
        Si usas ese mismo DbContext para comandos/updates y tu patrón es:
        Cargar entidad → modificar propiedades → SaveChanges()
        Con NoTracking global, esa entidad vendrá desacoplada, y no se guardará salvo que la adjuntes(Attach/Update) o cambies la estrategia.
        Si dependes de fixup de navegación y relaciones automáticas mientras trabajas en memoria.
        Si haces updates parciales y te apoyas en el tracking para detectar cambios.
        Recomendación práctica (la que menos problemas da)
        No lo pongas global si ese AppDbContext también se usa para escritura.
        En su lugar, usa AsNoTracking() en las consultas de lectura (o AsNoTrackingWithIdentityResolution() si necesitas evitar duplicados de referencias en resultados complejos).





        Advertencias importantes al usar DbContexts con pool(conviene saberlo):
        Debes tratar el DbContext como “reutilizado”

        Cualquier estado que metas en la instancia del contexto(campos personalizados, estado no reiniciado) 
        puede “filtrarse” entre usos. Mantén el contexto sin estado (stateless).

        Cuidado con el tracking y los contextos con pool
        El pooling reinicia el estado interno de EF, pero el patrón más seguro sigue siendo: crear contexto → hacer el trabajo → disponerlo rápido.

        No captures un contexto del pool para reutilizarlo
        Solicita/crea siempre uno por unidad de trabajo.






        Por qué NO se suele usar pooling también para escrituras
        1) En escrituras, el DbContext suele vivir más y cambia mucho de estado
        Tracking de entidades, ChangeTracker, transacciones, SaveChanges, etc.
        El pooling brilla cuando el contexto es corto y repetitivo (muchas lecturas pequeñas). 
        En escritura es más fácil que haya “estado” y patrones variados, y el beneficio suele ser menor.

        2) Riesgo de “estado residual” y bugs sutiles
        EF Core intenta resetear el contexto cuando vuelve al pool, pero en escritura es más probable que:
        adjuntes entidades, uses BeginTransaction, uses interceptors/event handlers, manipules el ChangeTracker,
        tengas servicios/propiedades custom en el DbContext, por eso aumenta el riesgo de sorpresas 
        si alguien hace algo “no estándar”.

        3) La escritura suele estar naturalmente limitada
        En la mayoría de APIs hay muchas más lecturas que escrituras. Optimizar lecturas suele dar más retorno.

        4) Scoped DbContext encaja mejor con “unit of work”
        Para comandos, un DbContext scoped por request/handler es exactamente lo que quieres: una unidad de trabajo clara.
        
        */
    }
}
