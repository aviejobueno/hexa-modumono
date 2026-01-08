using System.Linq.Expressions;
using Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Abstractions;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Dtos;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Mapping;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Queries;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Queries.Common;
using Arch.Hexa.ModuMono.Modules.Customers.Domain;
using MediatR;

namespace Arch.Hexa.ModuMono.Modules.Customers.Application.Handlers;

public sealed class GetCustomersQueryHandler(ICustomerReadRepository<Customer, Guid> readRepository, ICustomersMapping mapping)
    : IRequestHandler<GetCustomersQuery, PagedResponse<CustomerDto?>>
{
    public async Task<PagedResponse<CustomerDto?>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        // query state
        var queryState = new QueryState<Customer>
        {
            Skip = request.PageNumber,
            Take = request.PageSize,
            Criteria = null,
            Sort = []
        };

        // filter criteria
        var hasAnyFilter =
            !string.IsNullOrWhiteSpace(request.NameFilter) ||
            !string.IsNullOrWhiteSpace(request.EmailFilter) ||
            request.StatusFilter.HasValue ||
            request.CreatedFrom.HasValue ||
            request.CreatedTo.HasValue;

        if (hasAnyFilter)
        {
            Expression<Func<Customer, bool>>  criteria = c =>
                (string.IsNullOrWhiteSpace(request.NameFilter) || c.Name.Contains(request.NameFilter)) &&
                (string.IsNullOrWhiteSpace(request.EmailFilter) || c.Email.Contains(request.EmailFilter)) &&
                (!request.StatusFilter.HasValue || c.Status == request.StatusFilter.Value) &&
                (!request.CreatedFrom.HasValue || c.CreatedAt >= request.CreatedFrom.Value) &&
                (!request.CreatedTo.HasValue || c.CreatedAt <= request.CreatedTo.Value);

            queryState = queryState with { Criteria = criteria };
        }

        // apply sorting
        var sortExpressions = CustomersSorting.ToSortExpressions(request.SortBy, request.Descending);
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
            return new PagedResponse<CustomerDto?>
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
            return new PagedResponse<CustomerDto?>
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