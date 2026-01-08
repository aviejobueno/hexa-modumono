using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Arch.Hexa.ModuMono.Modules.Orders.Infrastructure.Persistence;

/// <summary>
/// AppDbContextFactory for design-time DbContext creation when generate migrations
/// </summary>
public sealed class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    public OrderDbContext CreateDbContext(string[] args)
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
        
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new OrderDbContext(options);
    }
}