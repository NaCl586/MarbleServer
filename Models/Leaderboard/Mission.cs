namespace MarbleServer.Models.Leaderboard
{
    public class Mission
    {
        public int Id { get; set; }

        public int GameId { get; set; }

        public string? File { get; set; }

        public string Basename { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Gamemode { get; set; } = string.Empty;

        public bool IsCustom { get; set; }
    }
}