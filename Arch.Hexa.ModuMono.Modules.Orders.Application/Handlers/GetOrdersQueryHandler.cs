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

public sealed class GetOrdersQueryHandler(IOrderReadRepository<Order, Guid> readRepository, IOrdersMapping mapping)
    : IRequestHandler<GetOrdersQuery, PagedResponse<OrderDto?>>
{
    public async Task<PagedResponse<OrderDto?>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        // query state
        var queryState = new QueryState<Order>
        {
            Skip = request.PageNumber,
            Take = request.PageSize,
            Criteria = null,
            Sort = [],
            Includes =
            [
                o => o.OrderLines
            ]
        };

        // filter criteria
        var hasAnyFilter =
            request.CreatedFrom.HasValue ||
            request.CreatedTo.HasValue;

        if (hasAnyFilter)
        {
            Expression<Func<Order, bool>>  criteria = c =>
                (!request.CreatedFrom.HasValue || c.CreatedAt >= request.CreatedFrom.Value) &&
                (!request.CreatedTo.HasValue || c.CreatedAt <= request.CreatedTo.Value);

            queryState = queryState with { Criteria = criteria };
        }

        // apply sorting
        var sortExpressions = OrdersSorting.ToSortExpressions(request.SortBy, request.Descending);
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
            return new PagedResponse<OrderDto?>
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
            return new PagedResponse<OrderDto?>
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