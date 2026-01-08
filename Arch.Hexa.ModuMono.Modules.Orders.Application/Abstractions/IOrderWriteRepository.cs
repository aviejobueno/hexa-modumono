using Arch.Hexa.ModuMono.BuildingBlocks.Application.Abstractions;

namespace Arch.Hexa.ModuMono.Modules.Orders.Application.Abstractions;

public interface IOrderWriteRepository<in TEntity> : IWriteRepository<TEntity>
    where TEntity : class
{
    // Add order-specific write operations here if needed.
}

