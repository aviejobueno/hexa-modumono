namespace Arch.Hexa.ModuMono.BuildingBlocks.Application.Abstractions;

public interface ICustomerExistenceChecker
{
    Task<bool> CustomerExistAsync(Guid customerId, CancellationToken ct);
}