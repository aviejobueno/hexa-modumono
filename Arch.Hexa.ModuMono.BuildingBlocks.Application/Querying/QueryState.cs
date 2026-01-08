using System.Linq.Expressions;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying
{
    public sealed record QueryState<T>
    {
        public int Skip { get; init; } = 1;
        public int Take { get; init; } = 20;
        public Expression<Func<T, bool>>? Criteria { get; init; }
        public List<SortExpression<T>> Sort { get; init; } = [];
        public List<Expression<Func<T, object?>>> Includes { get; init; } = [];
    }
}
