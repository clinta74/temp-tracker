using System;

namespace temp_tracker.Models
{
    public record ReadingGraphData
    {
        public decimal Value { get; init; }
        public DateTime Taken { get; init; }
    }
}