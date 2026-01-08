using Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;
using Arch.Hexa.ModuMono.BuildingBlocks.Contracts.Enums;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Dtos;
using MediatR;

namespace Arch.Hexa.ModuMono.Modules.Customers.Application.Queries;

public sealed record GetCustomersQuery
(
    Dictionary<string, string[]>? Headers,
    int PageNumber,
    int PageSize,
    bool IncludeTotalCount,
    string? NameFilter,
    string? EmailFilter,
    CustomerStatus? StatusFilter,
    DateTime? CreatedFrom,
    DateTime? CreatedTo,
    string? SortBy,
    bool Descending

) : IRequest<PagedResponse<CustomerDto?>>;
