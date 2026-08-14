namespace VibeCheck.Service.Exceptions;

public class BadRequestException : ApiException
{
    public BadRequestException(string message) : base(message)
    {
    }

    public override int StatusCode => 400;
}
