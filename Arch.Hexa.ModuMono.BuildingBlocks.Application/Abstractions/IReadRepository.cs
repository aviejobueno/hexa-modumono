using Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Application.Abstractions
{
    public interface IReadRepository<TEntity, in TId> 
        where TEntity : class
    {
        Task<PagedResult<TEntity>> GetPagedAsync(QueryState<TEntity> queryState, CancellationToken cancellationToken = default);
        Task<SliceResult<TEntity>> GetSliceAsync(QueryState<TEntity> queryState, CancellationToken cancellationToken = default);
        Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    }
}
