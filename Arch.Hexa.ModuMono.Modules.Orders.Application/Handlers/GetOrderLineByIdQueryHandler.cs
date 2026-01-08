using Arch.Hexa.ModuMono.BuildingBlocks.Application.Exceptions;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Abstractions;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Dtos;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Mapping;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Queries;
using Arch.Hexa.ModuMono.Modules.Orders.Domain;
using MediatR;

namespace Arch.Hexa.ModuMono.Modules.Orders.Application.Handlers;

public sealed class GetOrderLineByIdQueryHandler(IOrderLineReadRepository<OrderLine, Guid> readRepository, IOrdersMapping mapping)
    : IRequestHandler<GetOrderLineByIdQuery, OrderLineDto?>
{
    public async Task<OrderLineDto?> Handle(GetOrderLineByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await readRepository.GetByIdAsync(request.Id, cancellationToken);
        return result == null ? throw new NotFoundException($"Order line with Id:{request.Id} not found.") : mapping.DomainToDto(result);
    }
}

