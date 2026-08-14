using System.Collections.Generic;

namespace MarbleServer.DTOs.Responses
{
    public class GameRatingLeaderboardResponse
    {
        public List<GameRatingResponse> Players { get; set; } =
            new();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalPlayers { get; set; }

        public int TotalPages { get; set; }
    }
}