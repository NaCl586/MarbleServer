namespace MarbleServer.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AchievementRating { get; set; }
        public ICollection<Score> Scores { get; set; } = new List<Score>();
    }
}