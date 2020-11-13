using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace temp_tracker.Models
{
    public record User
    {
        [Key]
        public int UserId { get; init; }
        public string Username { get; init; }
        public string Password { get; init; }
        public string Firstname { get; init;}
        public string Lastname { get; init; }
        public byte[] Salt { get; init; }
        public DateTime Created { get; init; }
        public virtual ICollection<UserRole> UserRoles { get; init; }
    }
}