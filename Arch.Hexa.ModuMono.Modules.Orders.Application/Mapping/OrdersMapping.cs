using Arch.Hexa.ModuMono.Modules.Orders.Application.Dtos;
using Arch.Hexa.ModuMono.Modules.Orders.Domain;

namespace Arch.Hexa.ModuMono.Modules.Orders.Application.Mapping;

public class OrdersMapping : IOrdersMapping
{
    public OrderDto DomainToDto(Order order)
    {
        return new()
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CreatedAt = order.CreatedAt,
            Lines = [.. order.OrderLines.Select(DomainToDto)],
            Total = order.Total
        };
    }

    public IEnumerable<OrderDto> DomainToDto(IEnumerable<Order> orders)
    {
        return orders.Select(DomainToDto);
    }

    public OrderLineDto DomainToDto(OrderLine orderLine)
    {
        return new()
        {
            Id = orderLine.Id,
            ProductName = orderLine.ProductName,
            Quantity = orderLine.Quantity,
            UnitPrice = orderLine.UnitPrice
        };
    }

    public IEnumerable<OrderLineDto> DomainToDto(IEnumerable<OrderLine> orderLines)
    {
        return orderLines.Select(DomainToDto);
    }
}