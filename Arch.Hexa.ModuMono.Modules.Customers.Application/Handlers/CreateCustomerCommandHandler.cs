using Arch.Hexa.ModuMono.BuildingBlocks.Application.Exceptions;
using Arch.Hexa.ModuMono.BuildingBlocks.Contracts.Enums;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Abstractions;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Commands;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Dtos;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Mapping;
using Arch.Hexa.ModuMono.Modules.Customers.Domain;
using MediatR;

namespace Arch.Hexa.ModuMono.Modules.Customers.Application.Handlers
{
    public sealed class CreateCustomerCommandHandler(ICustomerWriteRepository<Customer> writeRepository, ICustomerUnitOfWork unitOfWork, ICustomersMapping mapping) 
        : IRequestHandler<CreateCustomerCommand, CustomerDto?>
    {
        public async Task<CustomerDto?> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = new Customer(
                id: Guid.NewGuid(),
                name: request.Name,
                email: request.Email,
                status: CustomerStatus.Pending);

            await writeRepository.AddAsync(customer, cancellationToken);
            var result = await unitOfWork.SaveChangesAsync(cancellationToken);
            return result == 0 ? throw new ConflictException("Failed to create customer.") : mapping.DomainToDto(customer);
        }
    }
}