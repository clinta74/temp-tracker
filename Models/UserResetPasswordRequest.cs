using System.ComponentModel.DataAnnotations;

namespace temp_tracker.Models
{
    public class UserResetPasswordRequest
    {
        [Required]
        [MinLength(8)]
        public string Password { get; set; }

    }
}