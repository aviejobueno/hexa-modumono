using Arch.Hexa.ModuMono.Modules.Orders.Application.Dtos;
using MediatR;

namespace Arch.Hexa.ModuMono.Modules.Orders.Application.Queries;

public sealed record GetOrderLineByIdQuery(Dictionary<string, string[]>? Headers, Guid Id) : IRequest<OrderLineDto?>;

