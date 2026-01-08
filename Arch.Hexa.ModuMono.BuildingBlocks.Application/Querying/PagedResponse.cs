namespace Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;

public sealed class PagedResponse<T>
{
    public IEnumerable<T> Items { get; init; } = Array.Empty<T>();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }

    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }

    public int? TotalCount { get; init; }
    public int? TotalPages { get; init; }
}