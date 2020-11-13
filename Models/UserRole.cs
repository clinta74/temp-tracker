using System.Collections.Generic;

namespace temp_tracker.Models
{
    public record UserRole
    {
        public int UserId { get; init; }
        public int RoleId { get; init; }
        public virtual User User { get; init; }
        public virtual Role Role { get; init; }
    }
}