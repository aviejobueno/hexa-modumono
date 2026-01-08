namespace Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;

public class SortParser
{
    public static IReadOnlyList<SortField> Parse(string? sort, bool descending)
    {
        var list = new List<SortField>();

        if (string.IsNullOrWhiteSpace(sort))
            return list;

        var parts = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            var hasExplicitDirection = part.StartsWith('-');
            var key = hasExplicitDirection ? part[1..] : part;
            var isDescending = hasExplicitDirection || descending;
            list.Add(new SortField { Field = key, Descending = isDescending });
        }

        return list;
    }


}