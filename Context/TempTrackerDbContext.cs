
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using temp_tracker.Models;

namespace temp_tracker.Context
{
    public class TempTrackerDbContext : DbContext
    {
        public TempTrackerDbContext([NotNullAttribute] DbContextOptions options) : base(options)
        {
        }

        public DbSet<Reading> Readings { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}