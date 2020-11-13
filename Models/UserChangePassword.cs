using System.ComponentModel.DataAnnotations;

namespace temp_tracker.Models
{
    public record UserChangePasswordRequest
    {
        [Required]
        [MinLength(8)]
        public string Password { get; init; }

        [Required]
        public string OldPassword { get; init; }
    }
}