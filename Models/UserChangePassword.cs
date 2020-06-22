using System.ComponentModel.DataAnnotations;

namespace temp_tracker.Models
{
    public class UserChangePasswordRequest
    {
        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        public string OldPassword { get; set; }
    }
}