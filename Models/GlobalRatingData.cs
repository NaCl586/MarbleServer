namespace MarbleServer.Models
{
    public class GlobalRatingData
    {
        public int PlayerId { get; set; }
        public int MbgRating { get; set; }

        public int MbpRating { get; set; }

        public int MbpBonusRating { get; set; }

        public int GlobalRating { get; set; }

        public int AchievementRating { get; set; }
    }
}