namespace Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying
{
    public sealed class PagedResult<T>
    {
        public required IEnumerable<T> Items { get; init; }
        public required int PageNumber { get; init; }
        public required int PageSize { get; init; }
        public required int TotalCount { get; init; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
