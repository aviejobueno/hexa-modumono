using System.Linq.Expressions;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;

public sealed class SortExpression<T>
{
    public required Expression<Func<T, object?>> KeySelector { get; init; }
    public required bool Descending { get; init; }
}