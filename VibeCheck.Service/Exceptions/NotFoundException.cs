namespace VibeCheck.Service.Exceptions;

public class NotFoundException : ApiException
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key) : base($"{entityName} with id '{key}' was not found.")
    {
    }

    public override int StatusCode => 404;
}
