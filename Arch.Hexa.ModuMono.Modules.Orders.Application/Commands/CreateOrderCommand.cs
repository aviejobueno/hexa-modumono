using Arch.Hexa.ModuMono.Modules.Orders.Application.Dtos;
using MediatR;

namespace Arch.Hexa.ModuMono.Modules.Orders.Application.Commands;

public sealed record CreateOrderCommand(Dictionary<string, string[]>? Headers, Guid CustomerId, IEnumerable<OrderLineDto> Lines) : IRequest<OrderDto?>;

