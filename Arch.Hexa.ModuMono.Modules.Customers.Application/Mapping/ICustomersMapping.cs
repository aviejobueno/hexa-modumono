using Arch.Hexa.ModuMono.Modules.Customers.Application.Dtos;
using Arch.Hexa.ModuMono.Modules.Customers.Domain;

namespace Arch.Hexa.ModuMono.Modules.Customers.Application.Mapping;

public interface ICustomersMapping
{
    // Domain to Dto
    CustomerDto DomainToDto(Customer customer);
    IEnumerable<CustomerDto> DomainToDto(IEnumerable<Customer> customers);
}