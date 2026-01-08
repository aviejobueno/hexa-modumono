using Arch.Hexa.ModuMono.BuildingBlocks.Infrastructure.Persistence.Repositories;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Abstractions;

namespace Arch.Hexa.ModuMono.Modules.Customers.Infrastructure.Persistence.Repositories
{
    public sealed class CustomerWriteRepository<TEntity>(CustomerDbContext dbContext) 
        : GenericWriteRepository<TEntity, CustomerDbContext>(dbContext), ICustomerWriteRepository<TEntity>
        where TEntity : class
    {

        // Add customer-specific write methods here if needed.
    }
}
