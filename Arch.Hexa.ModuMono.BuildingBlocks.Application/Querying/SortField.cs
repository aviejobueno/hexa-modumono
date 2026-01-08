namespace Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying
{
    public sealed class SortField
    {
        public string Field { get; init; } = null!;
        public bool Descending { get; init; }
    }
}
