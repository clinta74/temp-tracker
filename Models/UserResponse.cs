using System;
using System.Collections.Generic;

namespace temp_tracker.Models
{
    public class UserResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public DateTime Created { get; set; }
        public IEnumerable<Role> Roles { get; set; }
    }
}