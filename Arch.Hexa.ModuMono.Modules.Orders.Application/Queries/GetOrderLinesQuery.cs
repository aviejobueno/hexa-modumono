using Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Dtos;
using MediatR;

namespace Arch.Hexa.ModuMono.Modules.Orders.Application.Queries;

public sealed record GetOrderLinesQuery
(
    Dictionary<string, string[]>? Headers,
    int PageNumber,
    int PageSize,
    bool IncludeTotalCount,
    string? ProductNameFilter,
    int? QuantityFilter,
    decimal? UnitPriceFilter,
    Guid? OrderIdFilter,
    string? SortBy,
    bool Descending

) : IRequest<PagedResponse<OrderLineDto?>>;
