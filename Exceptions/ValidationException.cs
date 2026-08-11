using System.Net;

namespace MarbleServer.Exceptions
{
    public class ValidationException : ApiException
    {
        public ValidationException(string message)
            : base(
                (int)HttpStatusCode.BadRequest,
                message)
        {
        }
    }
}