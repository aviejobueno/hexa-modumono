using Arch.Hexa.ModuMono.BuildingBlocks.Application.Exceptions;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Abstractions;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Dtos;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Mapping;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Queries;
using Arch.Hexa.ModuMono.Modules.Customers.Domain;
using MediatR;

namespace Arch.Hexa.ModuMono.Modules.Customers.Application.Handlers
{
    public sealed class GetCustomerByIdQueryHandler(ICustomerReadRepository<Customer, Guid> readRepository, ICustomersMapping mapping) 
        : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
    {
        public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await readRepository.GetByIdAsync(request.Id, cancellationToken);
            return result == null ? throw new NotFoundException($"Customer with Id:{request.Id} not found.") : mapping.DomainToDto(result);
        }
    }
}
