using Arch.Hexa.ModuMono.Modules.Customers.Application.Dtos;
using Arch.Hexa.ModuMono.Modules.Customers.Domain;

namespace Arch.Hexa.ModuMono.Modules.Customers.Application.Mapping;

public sealed class CustomersMapping : ICustomersMapping
{
    public CustomerDto DomainToDto(Customer customer)
    {
        return new()
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Status = customer.Status,
            CreatedAt = customer.CreatedAt
        };
    }


    public IEnumerable<CustomerDto> DomainToDto(IEnumerable<Customer> customers)
    {
        return customers.Select(DomainToDto);
    }
}