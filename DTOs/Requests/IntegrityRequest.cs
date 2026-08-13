using System.ComponentModel.DataAnnotations;

namespace MarbleServer.DTOs.Requests
{
    public class IntegrityRequest
    {
        [Required]
        public string GameVersion { get; set; } = string.Empty;

        [Required]
        public List<string> Files { get; set; } =
            new List<string>();
    }
}