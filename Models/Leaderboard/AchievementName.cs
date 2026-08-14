namespace MarbleServer.Models.Leaderboard
{
    public class AchievementName
    {
        public int Id { get; set; }

        public int? CategoryId { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public int? Rating { get; set; }
    }
}