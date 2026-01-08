using Arch.Hexa.ModuMono.Modules.Orders.Application.Dtos;
using Arch.Hexa.ModuMono.Modules.Orders.Domain;

namespace Arch.Hexa.ModuMono.Modules.Orders.Application.Mapping;

public interface IOrdersMapping
{
    // Domain to Dto

    // Order
    OrderDto DomainToDto(Order order);
    IEnumerable<OrderDto> DomainToDto(IEnumerable<Order> orders);

    // OrderLine
    OrderLineDto DomainToDto(OrderLine orderLine);
    IEnumerable<OrderLineDto> DomainToDto(IEnumerable<OrderLine> orderLines);

}