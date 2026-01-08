using Arch.Hexa.ModuMono.Orders.RestApi.Contracts.Requests.Order;
using FluentValidation;

namespace Arch.Hexa.ModuMono.Orders.RestApi.Contracts.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .NotNull()
            .WithMessage("The customer ID is required.");

        RuleFor(x => x.Lines)
            .NotEmpty()
            .NotNull()
            .WithMessage("The order lines are required.");
    }
}