using Arch.Hexa.ModuMono.Modules.Customers.Application.Abstractions;

namespace Arch.Hexa.ModuMono.Modules.Customers.Infrastructure.Persistence
{
    public class CustomerUnitOfWork(CustomerDbContext dbContext) : ICustomerUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
