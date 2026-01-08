using Arch.Hexa.ModuMono.BuildingBlocks.Application.Abstractions;

namespace Arch.Hexa.ModuMono.Modules.Customers.Application.Abstractions
{
    public interface ICustomerReadRepository<TEntity, in TId> : IReadRepository<TEntity, TId>
        where TEntity : class
    {
        // If you ever need specific methods for reading Customer data,
        // You add them here without breaking the generic contract.
    }
}
