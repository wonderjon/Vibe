namespace VibeCheck.Service.Exceptions;

/// <summary>
/// Base type for exceptions that carry an HTTP status code, so the API's global
/// exception middleware can translate them into the right response without
/// controllers needing to know about status codes at all.
/// </summary>
public abstract class ApiException : Exception
{
    protected ApiException(string message) : base(message)
    {
    }

    public abstract int StatusCode { get; }
}
