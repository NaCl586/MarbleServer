namespace MarbleServer.Models;

public class Replay
{
    public int Id { get; set; }
    public int ScoreId { get; set; }
    public Score Score { get; set; } = null!;
    public string FileName { get; set; } = "";
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}