
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace temp_tracker.Models
{
    public class Reading
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ReadingId { get; set; }
        public decimal Value { get; set; }
        public int Scale { get; set; }
        public DateTime Taken { get; set; }
    }

    public class ReadingRequest
    {
        public decimal Value { get; set; }
        public int Scale { get; set; }
        public DateTime? Taken { get; set; }
    }

    public enum Scales { F, C }
}