using Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;
using Arch.Hexa.ModuMono.Modules.Orders.Domain;

namespace Arch.Hexa.ModuMono.Modules.Orders.Application.Queries.Common;

public static class OrderLinesSorting
{
    public static List<SortExpression<OrderLine>> ToSortExpressions(string? sort, bool descending)
    {
        var fields = SortParser.Parse(sort, descending);
        var list = new List<SortExpression<OrderLine>>();

        if (fields.Count == 0)
        {
            list.Add(new SortExpression<OrderLine>      
            {
                KeySelector = c => c.ProductName,
                Descending = true
            });

            return list;
        }

        foreach (var field in fields)
        {
            var key = field.Field.ToLowerInvariant();


            if (key == "productName")
            {
                list.Add(new SortExpression<OrderLine>
                {
                    KeySelector = c => c.ProductName,
                    Descending = field.Descending
                });
            }
            else if (key == "quantity")
            {
                list.Add(new SortExpression<OrderLine>
                {
                    KeySelector = c => c.Quantity,
                    Descending = field.Descending
                });
            }
            else if (key == "unitPrice")
            {
                list.Add(new SortExpression<OrderLine>
                {
                    KeySelector = c => c.UnitPrice,
                    Descending = field.Descending
                });
            }
            else if (key == "orderId")
            {
                list.Add(new SortExpression<OrderLine>
                {
                    KeySelector = c => c.OrderId,
                    Descending = field.Descending
                });
            }
        }

        if (list.Count == 0)
        {
            list.Add(new SortExpression<OrderLine>
            {
                KeySelector = c => c.ProductName,
                Descending = true
            });
        }

        return list;
    }
}