namespace Arch.Hexa.ModuMono.Orders.RestApi.Contracts.Requests.Order;

public class CreateOrderLineRequest
{
    public required string ProductName { get; init; } = string.Empty;
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
}