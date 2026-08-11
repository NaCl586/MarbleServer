namespace MarbleServer.DTOs.Responses
{
    public class MyRankResponse
    {
        public int Rank { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int TimeMs { get; set; }
        public int TotalPlayers { get; set; }
    }
}