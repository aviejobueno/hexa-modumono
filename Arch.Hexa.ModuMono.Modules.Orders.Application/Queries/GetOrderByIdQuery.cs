using Arch.Hexa.ModuMono.Modules.Orders.Application.Dtos;
using MediatR;

namespace Arch.Hexa.ModuMono.Modules.Orders.Application.Queries;

public sealed record GetOrderByIdQuery(Dictionary<string, string[]>? Headers, Guid Id) : IRequest<OrderDto?>;

