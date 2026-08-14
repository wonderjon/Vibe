namespace VibeCheck.Service.Exceptions;

/// <summary>
/// Wraps FluentValidation failures into an ApiException so the global middleware can
/// render them the same way as every other error, without the API layer depending on FluentValidation.
/// </summary>
public class AppValidationException : ApiException
{
    public AppValidationException(IReadOnlyDictionary<string, string[]> errors) : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public override int StatusCode => 400;
}
