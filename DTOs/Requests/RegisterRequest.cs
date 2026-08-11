using System.ComponentModel.DataAnnotations;

namespace MarbleServer.DTOs.Requests
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(32, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(64, MinimumLength = 6)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9\s])\S+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one symbol.")]
        public string Password { get; set; } = string.Empty;
    }
}