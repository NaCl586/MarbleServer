namespace MarbleServer.Exceptions
{
    public class ReplayAlreadyExistsException : Exception
    {
        public ReplayAlreadyExistsException(int scoreId)
            : base($"Replay for score {scoreId} already exists.")
        {
        }
    }
}
