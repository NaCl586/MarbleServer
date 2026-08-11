using MarbleServer.Exceptions;

public class NotFoundException : ApiException
{
    public NotFoundException(string entity, object key)
        : base(
            StatusCodes.Status404NotFound,
            $"{entity} '{key}' was not found.")
    {
    }

    public NotFoundException(string message)
        : base(
            StatusCodes.Status404NotFound,
            message)
    {
    }
}