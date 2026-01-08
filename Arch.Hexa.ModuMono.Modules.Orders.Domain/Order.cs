using Arch.Hexa.ModuMono.BuildingBlocks.Domain.Abstractions;

namespace Arch.Hexa.ModuMono.Modules.Orders.Domain
{
    public sealed class Order() : IEntity
    {
        private readonly List<OrderLine> _lines = [];

        public Order(Guid id, Guid customerId) : this()
        {
            Id = id;
            CustomerId = customerId;
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public decimal Total => _lines.Sum(l => l.Quantity * l.UnitPrice);


        public ICollection<OrderLine> OrderLines => _lines;
        

        public void AddLine(Guid orderId, string productName, int quantity, decimal unitPrice)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
            ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);

            _lines.Add(new OrderLine(Guid.NewGuid(), orderId, productName, quantity, unitPrice));
        }
    }
}
