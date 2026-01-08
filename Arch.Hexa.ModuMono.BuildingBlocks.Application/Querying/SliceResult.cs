namespace Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;

public sealed class SliceResult<T>
{
    public required IEnumerable<T> Items { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
    public required bool HasNextPage { get; init; }
    public bool HasPreviousPage => PageNumber > 1;
}