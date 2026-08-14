namespace MarbleServer.DTOs.Requests
{
    public class CalculateRatingsRequest
    {
        public string Level { get; set; } = string.Empty;

        public List<int> TimesMs { get; set; } = new();
    }
}