using System.ComponentModel.DataAnnotations;

namespace MarbleServer.DTOs.Requests
{
    public class SubmitScoreRequest
    {
        [Required]
        [StringLength(100)]
        public string Level { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int TimeMs { get; set; }
    }
}