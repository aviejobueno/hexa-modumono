using Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;
using Arch.Hexa.ModuMono.Modules.Customers.Domain;

namespace Arch.Hexa.ModuMono.Modules.Customers.Application.Queries.Common;

public static class CustomersSorting
{
    public static List<SortExpression<Customer>> ToSortExpressions(string? sort, bool descending)
    {
        var fields = SortParser.Parse(sort, descending);
        var list = new List<SortExpression<Customer>>();

        if (fields.Count == 0)
        {
            list.Add(new SortExpression<Customer>
            {
                KeySelector = c => c.CreatedAt,
                Descending = true
            });

            return list;
        }

        foreach (var field in fields)
        {
            var key = field.Field.ToLowerInvariant();

            if (key == "name")
            {
                list.Add(new SortExpression<Customer>
                {
                    KeySelector = c => c.Name,
                    Descending = field.Descending
                });
            }
            else if (key == "email")
            {
                list.Add(new SortExpression<Customer>
                {
                    KeySelector = c => c.Email,
                    Descending = field.Descending
                });
            }
            else if (key == "status")
            {
                list.Add(new SortExpression<Customer>
                {
                    KeySelector = c => c.Status,
                    Descending = field.Descending
                });
            }
            else if (key == "createdat")
            {
                list.Add(new SortExpression<Customer>
                {
                    KeySelector = c => c.CreatedAt,
                    Descending = field.Descending
                });
            }
        }

        if (list.Count == 0)
        {
            list.Add(new SortExpression<Customer>
            {
                KeySelector = c => c.CreatedAt,
                Descending = true
            });
        }

        return list;
    }
}