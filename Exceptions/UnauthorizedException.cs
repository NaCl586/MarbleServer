using System.Net;

namespace MarbleServer.Exceptions
{
    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException(string message)
            : base(
                (int)HttpStatusCode.Unauthorized,
                message)
        {
        }
    }
}