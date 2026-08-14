namespace MarbleServer.Models.Leaderboard
{
    public class MissionRatingInfo
    {
        public int MissionId { get; set; }

        public int? ParTime { get; set; }

        public int? PlatinumTime { get; set; }

        public int? UltimateTime { get; set; }

        public int? CompletionBonus { get; set; }

        public int? SetBaseScore { get; set; }

        public double? MultiplierSetBase { get; set; }

        public int? PlatinumBonus { get; set; }

        public int? UltimateBonus { get; set; }

        public int? Standardiser { get; set; }

        public int? TimeOffset { get; set; }

        public double? Difficulty { get; set; }

        public double? PlatinumDifficulty { get; set; }

        public double? UltimateDifficulty { get; set; }

        public bool? Disabled { get; set; }
    }
}