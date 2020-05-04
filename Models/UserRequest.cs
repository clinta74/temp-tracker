using System.ComponentModel.DataAnnotations;

namespace temp_tracker.Models
{
    public class UserRequest
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        [MinLength(8)]
        public string Password { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }
}