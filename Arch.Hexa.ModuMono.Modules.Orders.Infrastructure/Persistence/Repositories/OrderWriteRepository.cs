using Arch.Hexa.ModuMono.BuildingBlocks.Infrastructure.Persistence.Repositories;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Abstractions;

namespace Arch.Hexa.ModuMono.Modules.Orders.Infrastructure.Persistence.Repositories
{
    public sealed class OrderWriteRepository<TEntity>(OrderDbContext dbContext) 
        : GenericWriteRepository<TEntity, OrderDbContext>(dbContext), IOrderWriteRepository<TEntity>
        where TEntity : class
    {

        // Add order-specific write methods here if needed.
    }
}
