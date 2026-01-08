using Arch.Hexa.ModuMono.BuildingBlocks.Contracts.Enums;
using Arch.Hexa.ModuMono.BuildingBlocks.Domain.Abstractions;

namespace Arch.Hexa.ModuMono.Modules.Customers.Domain
{
    public sealed class Customer() : IEntity
    {
        public Customer(Guid id, string name, string email, CustomerStatus status) : this()
        {
            Id = id;
            Name = name;
            Email = email;
            Status = status;
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public CustomerStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public void Activate() => Status = CustomerStatus.Active;
        public void Deactivate() => Status = CustomerStatus.Inactive;
    }
}
