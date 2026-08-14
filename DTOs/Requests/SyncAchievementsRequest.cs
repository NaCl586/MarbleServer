using System.Collections.Generic;

namespace MarbleServer.DTOs.Requests
{
    public class SyncAchievementsRequest
    {
        public List<int> AchievementIds { get; set; } = new();
    }
}