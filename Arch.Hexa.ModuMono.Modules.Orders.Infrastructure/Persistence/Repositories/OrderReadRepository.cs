using Arch.Hexa.ModuMono.BuildingBlocks.Infrastructure.Persistence.Repositories;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Abstractions;
using Arch.Hexa.ModuMono.Modules.Orders.Domain;
using Microsoft.EntityFrameworkCore;

namespace Arch.Hexa.ModuMono.Modules.Orders.Infrastructure.Persistence.Repositories
{
    public sealed class OrderReadRepository<TEntity, TId>(IDbContextFactory<OrderDbContext> factory)
        : GenericReadRepository<TEntity, OrderDbContext, TId>(factory), IOrderReadRepository<TEntity, TId>
        where TEntity : class
    {
        private readonly IDbContextFactory<OrderDbContext> _factory =
            factory ?? throw new ArgumentNullException(nameof(factory));

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await _factory.CreateDbContextAsync(cancellationToken);

            return await dbContext.Orders
                .Include(o => o.OrderLines)
                .SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
        }
    }
}
