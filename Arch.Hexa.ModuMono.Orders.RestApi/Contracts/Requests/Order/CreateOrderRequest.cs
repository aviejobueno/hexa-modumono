namespace Arch.Hexa.ModuMono.Orders.RestApi.Contracts.Requests.Order
{
    public class CreateOrderRequest
    {
        public required Guid CustomerId { get; init; }
        public required IEnumerable<CreateOrderLineRequest> Lines { get; init; } = [];
    }
}
