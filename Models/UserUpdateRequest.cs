using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace temp_tracker.Models
{
    public class UserUpdateRequest
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public IEnumerable<string> Roles { get; set; }

    }
}