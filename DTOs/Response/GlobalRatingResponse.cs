namespace MarbleServer.DTOs.Responses
{
    public class GlobalRatingResponse
    {
        public int GlobalRank { get; set; }

        public int MbgRank { get; set; }

        public int MbpRank { get; set; }

        public int MbpBonusRank { get; set; }

        public string PlayerName { get; set; }
            = string.Empty;

        public int MbgRating { get; set; }

        public int MbpRating { get; set; }

        public int MbpBonusRating { get; set; }

        public int GlobalRating { get; set; }

        public int PlayerId { get; set; }
        public int AchievementRating { get; set; }
    }
}