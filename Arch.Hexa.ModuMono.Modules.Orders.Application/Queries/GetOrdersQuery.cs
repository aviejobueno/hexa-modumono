using Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Dtos;
using MediatR;

namespace Arch.Hexa.ModuMono.Modules.Orders.Application.Queries;

public sealed record GetOrdersQuery
(
    Dictionary<string, string[]>? Headers,
    int PageNumber,
    int PageSize,
    bool IncludeTotalCount,
    string? CustomerIdFilter,
    DateTime? CreatedFrom,
    DateTime? CreatedTo,
    string? SortBy,
    bool Descending

) : IRequest<PagedResponse<OrderDto?>>;
