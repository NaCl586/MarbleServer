namespace MarbleServer.DTOs.Responses
{
    public class LeaderboardResponse
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<ScoreResponse> Scores { get; set; }
            = new();
    }
}