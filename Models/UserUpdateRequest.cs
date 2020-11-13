using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace temp_tracker.Models
{
    public record UserUpdateRequest
    {
        public string Firstname { get; init; }
        public string Lastname { get; init; }
        public IEnumerable<string> Roles { get; init; }

    }
}