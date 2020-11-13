using System.ComponentModel.DataAnnotations;

namespace temp_tracker.Models
{
    public record UserResetPasswordRequest
    {
        [Required]
        [MinLength(8)]
        public string Password { get; init; }

    }
}