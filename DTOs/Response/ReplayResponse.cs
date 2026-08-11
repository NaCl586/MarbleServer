namespace MarbleServer.DTOs.Responses;

public class ReplayResponse
{
    public int Id { get; set; }
    public int ScoreId { get; set; }
    public string FileName { get; set; } = "";
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}