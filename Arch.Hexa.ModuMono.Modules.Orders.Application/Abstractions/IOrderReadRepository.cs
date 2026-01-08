using Arch.Hexa.ModuMono.BuildingBlocks.Application.Abstractions;

namespace Arch.Hexa.ModuMono.Modules.Orders.Application.Abstractions;

public interface IOrderReadRepository<TEntity, in TId> : IReadRepository<TEntity, TId>
    where TEntity : class
{
    // If you ever need specific methods for reading Order data,
    // You add them here without breaking the generic contract.
}

