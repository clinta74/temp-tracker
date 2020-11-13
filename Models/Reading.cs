
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace temp_tracker.Models
{
    public record Reading
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ReadingId { get; init; }

        [Column(TypeName = "decimal(18,4)")]

        public decimal Value { get; init; }

        public int Scale { get; init; }

        public DateTime Taken { get; init; }
    }

    public record ReadingRequest
    {
        public decimal Value { get; init; }
        public int Scale { get; init; }
        public DateTime? Taken { get; init; }
    }

    public enum Scales { F, C }
}