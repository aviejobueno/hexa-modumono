using System.Text.RegularExpressions;
using Arch.Hexa.ModuMono.Customers.RestApi.Contracts.Requests.Customer;
using FluentValidation;

namespace Arch.Hexa.ModuMono.Customers.RestApi.Contracts.Validators;

public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{

    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);


    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .NotNull()
            .WithMessage("The customer name is required.")
            .MaximumLength(75)
            .WithMessage("The customer name has a maximum length of 75 characters.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("The customer email is required.")
            .Must(static email => EmailRegex.IsMatch(email))
            .WithMessage("The customer email is not valid.")
            .MaximumLength(100)
            .WithMessage("The customer email has a maximum length of 100 characters.");

    }


}