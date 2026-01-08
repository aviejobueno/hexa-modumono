using Arch.Hexa.ModuMono.BuildingBlocks.Infrastructure.Persistence.Repositories;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Arch.Hexa.ModuMono.Modules.Orders.Infrastructure.Persistence.Repositories
{
    public sealed class OrderLineReadRepository<TEntity, TId>(IDbContextFactory<OrderDbContext> factory)
        : GenericReadRepository<TEntity, OrderDbContext, TId>(factory), IOrderLineReadRepository<TEntity, TId>
        where TEntity : class
    {
        // Add order-specific read methods here if needed.
    }
}
