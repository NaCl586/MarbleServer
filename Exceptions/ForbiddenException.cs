using System.Net;

namespace MarbleServer.Exceptions
{
    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string message)
            : base(
                (int)HttpStatusCode.Forbidden,
                message)
        {
        }
    }
}