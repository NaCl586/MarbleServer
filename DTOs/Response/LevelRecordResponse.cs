namespace MarbleServer.DTOs.Responses
{
    public class LevelRecordResponse
    {
        public string Level { get; set; } = string.Empty;

        public bool HasPersonalRecord { get; set; }

        public int? PersonalTimeMs { get; set; }

        public int? PersonalRating { get; set; }

        public bool HasGlobalRecord { get; set; }

        public string? GlobalPlayerName { get; set; }

        public int? GlobalTimeMs { get; set; }

        public int? GlobalRating { get; set; }
    }
}