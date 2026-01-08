using Arch.Hexa.ModuMono.Modules.Customers.Application.Dtos;
using MediatR;

namespace Arch.Hexa.ModuMono.Modules.Customers.Application.Commands
{
    public sealed record CreateCustomerCommand(Dictionary<string, string[]>? Headers, string Name, string Email) : IRequest<CustomerDto?>;
}
