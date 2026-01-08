using Arch.Hexa.ModuMono.BuildingBlocks.Application.Abstractions;

namespace Arch.Hexa.ModuMono.Modules.Customers.Application.Abstractions
{
    public interface ICustomerWriteRepository<in TEntity> : IWriteRepository<TEntity>
        where TEntity : class
    {
        // Add customer-specific write operations here if needed.
    }
}
