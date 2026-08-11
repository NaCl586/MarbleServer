using Microsoft.EntityFrameworkCore;

namespace MarbleServer.Models;

[Index(nameof(PlayerId), nameof(Level), IsUnique = true)]
public class Score
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public string Level { get; set; } = "";
    public int TimeMs { get; set; }
    public DateTime SubmittedAt { get; set; }
    public Replay? Replay { get; set; }
}