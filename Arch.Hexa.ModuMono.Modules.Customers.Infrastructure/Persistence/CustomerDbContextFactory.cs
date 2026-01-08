using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Arch.Hexa.ModuMono.Modules.Customers.Infrastructure.Persistence;

/// <summary>
/// CustomerDbContextFactory for design-time DbContext creation when generate migrations
/// </summary>
public sealed class CustomerDbContextFactory : IDesignTimeDbContextFactory<CustomerDbContext>
{
    public CustomerDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)         
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Missing connection string 'DefaultConnection'. " +
                "Add it to appsettings.json or set ConnectionStrings__DefaultConnection env var.");

        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new CustomerDbContext(options);
    }
}