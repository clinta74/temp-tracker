
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

        public DbSet<Role> Roles { get; set; }

        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            builder.Entity<UserRole>()
                .HasKey(c => new { c.RoleId, c.UserId });

            builder.Entity<UserRole>()
                .HasOne<User>(ur => ur.User)
                .WithMany(s => s.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            builder.Entity<UserRole>()
                .HasOne<Role>(ur => ur.Role)
                .WithMany(s => s.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

        }
    }
}