namespace VibeCheck.Service.Exceptions;

public class UnauthorizedException : ApiException
{
    public UnauthorizedException(string message) : base(message)
    {
    }

    public override int StatusCode => 401;
}
