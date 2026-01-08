namespace Arch.Hexa.ModuMono.Modules.Orders.Application.Dtos;

public sealed class OrderLineDto
{
    public Guid Id { get; init; }
    public string ProductName { get; init; } = null!;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}

