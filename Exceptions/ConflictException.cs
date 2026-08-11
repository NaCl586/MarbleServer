using System.Net;

namespace MarbleServer.Exceptions
{
    public class ConflictException : ApiException
    {
        public ConflictException(string message)
            : base(
                (int)HttpStatusCode.Conflict,
                message)
        {
        }
    }
}