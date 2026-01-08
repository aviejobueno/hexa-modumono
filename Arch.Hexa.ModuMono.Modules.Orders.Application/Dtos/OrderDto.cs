namespace Arch.Hexa.ModuMono.Modules.Orders.Application.Dtos;

public class OrderDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime CreatedAt { get; init; }
    public IEnumerable<OrderLineDto> Lines { get; init; } = null!;
    public decimal Total { get; init; }
}