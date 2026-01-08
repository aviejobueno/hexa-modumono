using System.Text;
using FluentValidation;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Api.Helpers;

public static class ValidationsHelper
{
    public static async Task<string?> Validations<T>(IValidator<T> validator, T item)
    {
        var validationResult = await (validator.ValidateAsync(item))!;
        var validationErrors = new StringBuilder();

        if (validationResult.IsValid) return null;

        foreach (var error in validationResult.Errors)
        {
            validationErrors.Append($"[{error.PropertyName} : {error.ErrorMessage}]");
        }
        return validationErrors.ToString();
    }
}