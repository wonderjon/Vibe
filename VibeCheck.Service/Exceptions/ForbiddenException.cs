namespace VibeCheck.Service.Exceptions;

public class ForbiddenException : ApiException
{
    public ForbiddenException(string message) : base(message)
    {
    }

    public override int StatusCode => 403;
}
