namespace MarbleServer.Models.Leaderboard
{
    public class MissionGame
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Display { get; set; }

        public string? LongDisplay { get; set; }

        public string? RatingColumn { get; set; }

        public string? GameType { get; set; }
    }
}