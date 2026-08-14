using FluentValidation;
using VibeCheck.Service.Exceptions;

namespace VibeCheck.Service.Validators;

public static class ValidatorExtensions
{
    /// <summary>
    /// Runs a FluentValidation validator and, on failure, throws AppValidationException instead of
    /// FluentValidation.ValidationException — keeps the API layer's error handling framework-agnostic.
    /// </summary>
    public static async Task ValidateAndThrowAppAsync<T>(this IValidator<T> validator, T instance, CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (result.IsValid)
            return;

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        throw new AppValidationException(errors);
    }
}
