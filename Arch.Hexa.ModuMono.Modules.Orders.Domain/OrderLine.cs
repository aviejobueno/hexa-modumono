using Arch.Hexa.ModuMono.BuildingBlocks.Domain.Abstractions;

namespace Arch.Hexa.ModuMono.Modules.Orders.Domain
{
    public sealed class OrderLine() : IEntity
    {
        public OrderLine(Guid id, Guid orderId, string productName, int quantity, decimal unitPrice) : this()
        {
            Id = id;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
            OrderId = orderId;
        }

        public Guid Id { get; private set; }
        public string ProductName { get; private set; } = null!;
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public Guid OrderId { get; private set; }

        public Order? Order { get; private set; } = null!;

    }
}
