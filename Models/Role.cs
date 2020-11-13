using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace temp_tracker.Models
{
    public record Role
    {
        [Key]
        public int RoleId { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public virtual ICollection<UserRole> UserRoles { get; init; }
    }
}