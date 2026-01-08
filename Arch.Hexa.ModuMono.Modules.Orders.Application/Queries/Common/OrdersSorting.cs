using Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;
using Arch.Hexa.ModuMono.Modules.Orders.Domain;

namespace Arch.Hexa.ModuMono.Modules.Orders.Application.Queries.Common;

public static class OrdersSorting
{
    public static List<SortExpression<Order>> ToSortExpressions(string? sort, bool descending)
    {
        var fields = SortParser.Parse(sort, descending);
        var list = new List<SortExpression<Order>>();

        if (fields.Count == 0)
        {
            list.Add(new SortExpression<Order>
            {
                KeySelector = c => c.CreatedAt,
                Descending = true
            });

            return list;
        }

        foreach (var field in fields)
        {
            var key = field.Field.ToLowerInvariant();

            if (key == "customerId")
            {
                list.Add(new SortExpression<Order>
                {
                    KeySelector = c => c.CustomerId,
                    Descending = field.Descending
                });
            }
            else if (key == "createdat")
            {
                list.Add(new SortExpression<Order>
                {
                    KeySelector = c => c.CreatedAt,
                    Descending = field.Descending
                });
            }
        }

        if (list.Count == 0)
        {
            list.Add(new SortExpression<Order>
            {
                KeySelector = c => c.CreatedAt,
                Descending = true
            });
        }

        return list;
    }
}