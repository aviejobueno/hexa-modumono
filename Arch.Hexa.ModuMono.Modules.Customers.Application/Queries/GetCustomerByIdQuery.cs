using Arch.Hexa.ModuMono.Modules.Customers.Application.Dtos;
using MediatR;

namespace Arch.Hexa.ModuMono.Modules.Customers.Application.Queries
{
    public sealed record GetCustomerByIdQuery(Dictionary<string, string[]>? Headers, Guid Id) : IRequest<CustomerDto?>;
}
