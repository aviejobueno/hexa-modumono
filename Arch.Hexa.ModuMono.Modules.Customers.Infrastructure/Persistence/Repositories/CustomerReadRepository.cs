using Arch.Hexa.ModuMono.BuildingBlocks.Application.Abstractions;
using Arch.Hexa.ModuMono.BuildingBlocks.Infrastructure.Persistence.Repositories;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Arch.Hexa.ModuMono.Modules.Customers.Infrastructure.Persistence.Repositories;

public sealed class CustomerReadRepository<TEntity, TId>(IDbContextFactory<CustomerDbContext> factory) 
    : GenericReadRepository<TEntity, CustomerDbContext, TId>(factory), ICustomerReadRepository<TEntity, TId>, ICustomerExistenceChecker
    where TEntity : class
{
    private readonly IDbContextFactory<CustomerDbContext> _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    public async Task<bool> CustomerExistAsync(Guid customerId, CancellationToken cancellationToken)
    {
        // This repository uses the pooled factory to create short-lived DbContexts (reads path)
        await using var dbContext = await _factory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Customers
            .AsNoTracking()
            .AnyAsync(x => x.Id == customerId, cancellationToken);
    }
}

    