using Arch.Hexa.ModuMono.BuildingBlocks.Application.Abstractions;
using Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;
using Microsoft.EntityFrameworkCore;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Infrastructure.Persistence.Repositories;

public class GenericReadRepository<TEntity, TContext, TId>(IDbContextFactory<TContext> factory)
        : IReadRepository<TEntity, TId>
        where TEntity : class
        where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _factory =
        factory ?? throw new ArgumentNullException(nameof(factory));

    public async Task<PagedResult<TEntity>> GetPagedAsync(QueryState<TEntity> queryState, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _factory.CreateDbContextAsync(cancellationToken);

        var query = ApplyState(dbContext.Set<TEntity>(), queryState);

        var pageNumber = queryState.Skip;
        var pageSize = queryState.Take;

        var skipRows = pageSize > 0 ? (pageNumber - 1) * pageSize : 0;

        var total = await query.CountAsync(cancellationToken);

        var items = pageSize > 0
            ? await query.Skip(skipRows).Take(pageSize).ToListAsync(cancellationToken)
            : await query.ToListAsync(cancellationToken);

        return new PagedResult<TEntity>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total
        };
    }
    
    public async Task<SliceResult<TEntity>> GetSliceAsync(QueryState<TEntity> queryState, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _factory.CreateDbContextAsync(cancellationToken);

        var query = ApplyState(dbContext.Set<TEntity>(), queryState);

        var pageNumber = queryState.Skip;
        var pageSize = queryState.Take;

        var skipRows = pageSize > 0 ? (pageNumber - 1) * pageSize : 0;

        if (pageSize <= 0)
        {
            var all = await query.ToListAsync(cancellationToken);

            return new SliceResult<TEntity>
            {
                Items = all,
                PageNumber = pageNumber,
                PageSize = pageSize,
                HasNextPage = false
            };
        }

        var itemsPlusOne = await query
            .Skip(skipRows)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        var hasNext = itemsPlusOne.Count > pageSize;

        var items = hasNext
            ? itemsPlusOne.Take(pageSize).ToList()
            : itemsPlusOne;

        return new SliceResult<TEntity>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            HasNextPage = hasNext
        };
    }

     /*
     public virtual async Task<PagedResult<TEntity>> GetAllAsync(QueryState<TEntity> queryState, CancellationToken cancellationToken = default)
      {
          await using var dbContext = await _factory.CreateDbContextAsync(cancellationToken);

          IQueryable<TEntity> query = dbContext.Set<TEntity>()
              .ApplyIncludes(queryState.Includes)
              .AsNoTracking()
              .AsQueryable();

          // Filter
          if (queryState.Criteria is not null)
              query = query.Where(queryState.Criteria);

          // Sort (first as OrderBy, the rest as ThenBy)
          var firstOrder = true;
          foreach (var sort in queryState.Sort)
          {
              if (firstOrder)
              {
                  query = sort.Descending
                      ? query.OrderByDescending(sort.KeySelector)
                      : query.OrderBy(sort.KeySelector);

                  firstOrder = false;
              }
              else
              {
                  var ordered = (IOrderedQueryable<TEntity>)query;
                  query = sort.Descending
                      ? ordered.ThenByDescending(sort.KeySelector)
                      : ordered.ThenBy(sort.KeySelector);
              }
          }

          // Pagination
          var pageNumber = queryState.Skip;
          var pageSize = queryState.Take;

          var skipRows = pageSize > 0
              ? (pageNumber - 1) * pageSize
              : 0;

          // If pageSize <= 0, treat as "return all" (no paging)
          if (pageSize <= 0)
          {
              var allItems = await query
                  .Skip(skipRows)
                  .ToListAsync(cancellationToken);

              return new PagedResult<TEntity>
              {
                  Items = allItems,
                  PageNumber = pageNumber,
                  PageSize = pageSize,
                  TotalCount = queryState.IncludeTotalCount ? allItems.Count : -1
              };
          }

          // Mode 1: compute TotalCount (2 queries)
          if (queryState.IncludeTotalCount)
          {
              var total = await query.CountAsync(cancellationToken);

              var items = await query
                  .Skip(skipRows)
                  .Take(pageSize)
                  .ToListAsync(cancellationToken);

              return new PagedResult<TEntity>
              {
                  Items = items,
                  PageNumber = pageNumber,
                  PageSize = pageSize,
                  TotalCount = total
              };
          }

          // Mode 2: no TotalCount (1 query) => pageSize + 1 strategy
          var itemsPlusOne = await query
              .Skip(skipRows)
              .Take(pageSize + 1)
              .ToListAsync(cancellationToken);

          var hasNext = itemsPlusOne.Count > pageSize;

          var pageItems = hasNext
              ? itemsPlusOne.Take(pageSize).ToList()
              : itemsPlusOne;

          return new PagedResult<TEntity>
          {
              Items = pageItems,
              PageNumber = pageNumber,
              PageSize = pageSize,
              TotalCount = -1
          };
      }



      public virtual async Task<PagedResult<TEntity>> GetAllAsync(QueryState<TEntity> queryState, CancellationToken cancellationToken = default)
      {
          // This repository uses the pooled factory to create short-lived DbContexts (reads path)
          await using var dbContext = await _factory.CreateDbContextAsync(cancellationToken);


          IQueryable<TEntity> query = dbContext.Set<TEntity>()
              .ApplyIncludes(queryState.Includes)
              .AsNoTracking()
              .AsQueryable();

          // Filter
          if (queryState.Criteria is not null)
              query = query.Where(queryState.Criteria);

          // Sort (first as OrderBy, the rest as ThenBy)
          bool firstOrder = true;
          foreach (var sort in queryState.Sort)
          {
              if (firstOrder)
              {
                  query = sort.Descending
                      ? query.OrderByDescending(sort.KeySelector)
                      : query.OrderBy(sort.KeySelector);
                  firstOrder = false;
              }
              else
              {
                  var ordered = (IOrderedQueryable<TEntity>)query;
                  query = sort.Descending
                      ? ordered.ThenByDescending(sort.KeySelector)
                      : ordered.ThenBy(sort.KeySelector);
              }
          }

          var total = await query.CountAsync(cancellationToken);

          List<TEntity> items;
          if (queryState.Take > 0)
          {
              items = await query
                  .Skip((queryState.Skip - 1) * queryState.Take)
                  .Take(queryState.Take)
                  .ToListAsync(cancellationToken);
          }
          else
          {
              items = await query
                  .Skip((queryState.Skip - 1) * queryState.Take)
                  .ToListAsync(cancellationToken);
          }

          return new PagedResult<TEntity>
          {
              Items = items,
              PageNumber = queryState.Skip,
              PageSize = queryState.Take,
              TotalCount = total
          };
      }

      */

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        // This repository uses the pooled factory to create short-lived DbContexts (reads path)
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Set<TEntity>()
            .FindAsync([id!], cancellationToken);
    }
    
    private static IQueryable<TEntity> ApplyState(IQueryable<TEntity> query, QueryState<TEntity> queryState)
    {
        query = query
            .ApplyIncludes(queryState.Includes)
            .AsNoTracking();

        if (queryState.Criteria is not null)
            query = query.Where(queryState.Criteria);

        var firstOrder = true;
        foreach (var sort in queryState.Sort)
        {
            if (firstOrder)
            {
                query = sort.Descending
                    ? query.OrderByDescending(sort.KeySelector)
                    : query.OrderBy(sort.KeySelector);

                firstOrder = false;
            }
            else
            {
                var ordered = (IOrderedQueryable<TEntity>)query;
                query = sort.Descending
                    ? ordered.ThenByDescending(sort.KeySelector)
                    : ordered.ThenBy(sort.KeySelector);
            }
        }

        return query;
    }
}