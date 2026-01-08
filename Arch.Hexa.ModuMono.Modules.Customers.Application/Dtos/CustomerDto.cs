

using Arch.Hexa.ModuMono.BuildingBlocks.Contracts.Enums;

namespace Arch.Hexa.ModuMono.Modules.Customers.Application.Dtos
{
    public class CustomerDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public string Email { get; init; } = null!;
        public CustomerStatus Status { get; init; }
        public DateTime CreatedAt { get; init; }


    }
}
