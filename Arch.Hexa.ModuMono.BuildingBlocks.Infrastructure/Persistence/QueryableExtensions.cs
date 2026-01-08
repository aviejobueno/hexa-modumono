using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Infrastructure.Persistence;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyIncludes<T>(
        this IQueryable<T> query,
        IReadOnlyList<Expression<Func<T, object?>>> includes)
        where T : class
    {
        return includes.Aggregate(query, (current, include) => current.Include(include));
    }
}