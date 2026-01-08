using System.Linq.Expressions;
using Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Abstractions;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Dtos;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Mapping;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Queries;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Queries.Common;
using Arch.Hexa.ModuMono.Modules.Orders.Domain;
using MediatR;

namespace Arch.Hexa.ModuMono.Modules.Orders.Application.Handlers;

public sealed class GetOrderLinesQueryHandler(IOrderLineReadRepository<OrderLine, Guid> readRepository, IOrdersMapping mapping)
    : IRequestHandler<GetOrderLinesQuery, PagedResponse<OrderLineDto?>>
{
    public async Task<PagedResponse<OrderLineDto?>> Handle(GetOrderLinesQuery request, CancellationToken cancellationToken)
    {
        // query state
        var queryState = new QueryState<OrderLine>
        {
            Skip = request.PageNumber,
            Take = request.PageSize,
            Criteria = null,
            Sort = []
        };

        // filter criteria
        var hasAnyFilter =
            !string.IsNullOrWhiteSpace(request.ProductNameFilter) ||
            request.QuantityFilter.HasValue ||
            request.UnitPriceFilter.HasValue ||
            request.OrderIdFilter.HasValue;

        if (hasAnyFilter)
        {
            Expression<Func<OrderLine, bool>> criteria = c =>
                (string.IsNullOrWhiteSpace(request.ProductNameFilter) || c.ProductName.Contains(request.ProductNameFilter)) &&
                (!request.QuantityFilter.HasValue || c.Quantity == request.QuantityFilter.Value) &&
                (!request.UnitPriceFilter.HasValue || c.UnitPrice == request.UnitPriceFilter.Value);

            queryState = queryState with { Criteria = criteria };
        }

        // apply sorting
        var sortExpressions = OrderLinesSorting.ToSortExpressions(request.SortBy, request.Descending);
        foreach (var sortExpr in sortExpressions)
        {
            queryState.Sort.Add(sortExpr);
        }

        if (request.IncludeTotalCount)
        {
            //fetch data
            var paged = await readRepository.GetPagedAsync(queryState, cancellationToken);
            
            //map dto
            var dtos = mapping.DomainToDto(paged.Items).ToList();

            //response
            return new PagedResponse<OrderLineDto?>
            {
                Items = dtos,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize,
                HasPreviousPage = paged.HasPreviousPage,
                HasNextPage = paged.HasNextPage,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages
            };
        }
        else
        {
            //fetch data
            var slice = await readRepository.GetSliceAsync(queryState, cancellationToken);
            
            //map dto
            var dtos = mapping.DomainToDto(slice.Items).ToList();

            //response
            return new PagedResponse<OrderLineDto?>
            {
                Items = dtos,
                PageNumber = slice.PageNumber,
                PageSize = slice.PageSize,
                HasPreviousPage = slice.HasPreviousPage,
                HasNextPage = slice.HasNextPage,
                TotalCount = null,
                TotalPages = null
            };
        }
    }
}       