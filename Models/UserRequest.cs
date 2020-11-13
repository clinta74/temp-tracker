using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace temp_tracker.Models
{
    public record UserRequest
    {
        [Required]
        public string Username { get; init; }
        
        [Required]
        [MinLength(8)]
        public string Password { get; init; }
        public string Firstname { get; init; }
        public string Lastname { get; init; }
        public IEnumerable<string> Roles { get; init; }
    }
}