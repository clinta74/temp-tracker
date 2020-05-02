using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace temp_tracker.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public byte[] Salt { get; set; }
        public DateTime Created { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}