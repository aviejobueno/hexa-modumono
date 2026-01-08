using Arch.Hexa.ModuMono.BuildingBlocks.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Infrastructure.Persistence.Repositories;

public class GenericWriteRepository<TEntity, TContext>(TContext dbContext) : IWriteRepository<TEntity>
    where TEntity : class
    where TContext : DbContext
{
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Remove(TEntity entity)
    {
        dbContext.Set<TEntity>().Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        dbContext.Set<TEntity>().RemoveRange(entities);
    }

    public virtual void Update(TEntity entity)
    {
        dbContext.Set<TEntity>().Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        dbContext.Set<TEntity>().UpdateRange(entities);
    }
}