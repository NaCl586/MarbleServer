namespace MarbleServer.DTOs.Responses
{
    public class GameRatingResponse
    {
        public int PlayerId { get; set; }

        public string PlayerName { get; set; } = string.Empty;

        public int Rating { get; set; }

        public int Rank { get; set; }
    }
}