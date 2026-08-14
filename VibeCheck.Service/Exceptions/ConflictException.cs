namespace VibeCheck.Service.Exceptions;

public class ConflictException : ApiException
{
    public ConflictException(string message) : base(message)
    {
    }

    public override int StatusCode => 409;
}
