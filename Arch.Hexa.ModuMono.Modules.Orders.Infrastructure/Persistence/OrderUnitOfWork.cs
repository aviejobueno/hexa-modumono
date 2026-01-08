using Arch.Hexa.ModuMono.Modules.Orders.Application.Abstractions;

namespace Arch.Hexa.ModuMono.Modules.Orders.Infrastructure.Persistence
{
    public class OrderUnitOfWork(OrderDbContext dbContext) : IOrderUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
