

namespace Arch.Hexa.ModuMono.BuildingBlocks.Application.Abstractions
{
    public interface IWriteRepository<in TEntity> 
        where TEntity : class
    {
        /// <summary>
        /// Adds a new entity to the change tracker.
        /// The actual persistence is done by calling IUnitOfWork.SaveChangesAsync().
        /// </summary>
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds multiple entities to the change tracker.
        /// </summary>
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks an entity as modified.
        /// </summary>
        void Update(TEntity entity);

        /// <summary>
        /// Marks multiple entities as modified.
        /// </summary>
        void UpdateRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Marks an entity for deletion.
        /// </summary>
        void Remove(TEntity entity);

        /// <summary>
        /// Marks multiple entities for deletion.
        /// </summary>
        void RemoveRange(IEnumerable<TEntity> entities);
    }
}
