namespace Arch.Hexa.ModuMono.Customers.RestApi.Contracts.Requests.Customer
{
    public class CreateCustomerRequest
    {
        public required string Name { get; set; } = string.Empty;
        public required string Email { get; set; } = string.Empty;
    }
}
