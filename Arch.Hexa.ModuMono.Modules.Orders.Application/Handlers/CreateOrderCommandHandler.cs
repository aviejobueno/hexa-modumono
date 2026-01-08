using Arch.Hexa.ModuMono.BuildingBlocks.Application.Abstractions;
using Arch.Hexa.ModuMono.BuildingBlocks.Application.Exceptions;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Abstractions;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Commands;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Dtos;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Mapping;
using Arch.Hexa.ModuMono.Modules.Orders.Domain;
using MediatR;

namespace Arch.Hexa.ModuMono.Modules.Orders.Application.Handlers;

public sealed class CreateOrderCommandHandler(IOrderWriteRepository<Order> writeRepository, IOrderUnitOfWork unitOfWork, IOrdersMapping mapping, ICustomerExistenceChecker customerExistence)
            : IRequestHandler<CreateOrderCommand, OrderDto?>
{
    public async Task<OrderDto?> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var exists = await customerExistence.CustomerExistAsync(request.CustomerId, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException($"Customer '{request.CustomerId}' does not exist. Failed to create order.");
        }

        var order = new Order(Guid.NewGuid(), request.CustomerId);

        foreach (var line in request.Lines)
        {
            order.AddLine(order.Id,line.ProductName, line.Quantity, line.UnitPrice);
        }

        await writeRepository.AddAsync(order, cancellationToken);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        return result == 0 ? throw new ConflictException("Failed to create order.") : mapping.DomainToDto(order);
    }
}

