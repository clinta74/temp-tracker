using System;
using System.Collections.Generic;

namespace temp_tracker.Models
{
    public record UserResponse
    {
        public int UserId { get; init; }
        public string Username { get; init; }
        public string Firstname { get; init; }
        public string Lastname { get; init; }
        public DateTime Created { get; init; }
        public IEnumerable<Role> Roles { get; init; }
    }
}