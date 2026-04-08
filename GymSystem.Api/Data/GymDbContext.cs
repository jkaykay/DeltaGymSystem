// ============================================================
// GymDbContext.cs — The database context ("gateway to the database").
// Entity Framework Core uses this class to know which tables exist
// and how they relate to each other. Every database query in the
// app goes through this context.
// ============================================================

using GymSystem.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Api.Data
{
    // Inherits from IdentityDbContext so it includes built-in tables
    // for users, roles, logins, etc. provided by ASP.NET Identity.
    public class GymDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        // Constructor — receives database options (connection string, provider)
        // from the dependency injection container (configured in Program.cs).
        public GymDbContext(DbContextOptions<GymDbContext> options) : base(options)
        {
        }

        // --- DbSet properties ---
        // Each DbSet represents a table in the database.
        // For example, "Tiers" maps to a "Tiers" table where each row is a Tier object.
        public DbSet<Tier> Tiers { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        // OnModelCreating lets us fine-tune how EF Core maps our models
        // to database tables — defining relationships, constraints, and conversions.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Always call base first so Identity tables are configured correctly.
            base.OnModelCreating(modelBuilder);

            // Store the SubscriptionState enum as a readable string ("Active", "Expired")
            // in the database instead of a raw integer.
            modelBuilder.Entity<Subscription>()
                .Property(s => s.State)
                .HasConversion<string>()
                .HasMaxLength(20);

            // --- Subscription relationships ---
            // Each Subscription belongs to one Tier (e.g. "Gold", "Silver").
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Tier)
                .WithMany(t => t.Subscriptions)
                .HasForeignKey(s => s.TierName)
                .OnDelete(DeleteBehavior.Cascade);

            // Each Subscription belongs to one User (the member).
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Payment relationships ---
            // Each Payment is linked to one Subscription.
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Subscription)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.SubId)
                .OnDelete(DeleteBehavior.Cascade);

            // Each Payment is also linked to one User.
            // NoAction prevents a cascade conflict with the Subscription cascade path.
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // --- User → Branch relationship ---
            // A user may optionally belong to a gym branch.
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Branch)
                .WithMany(b => b.Users)
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.NoAction);

            // --- Schedule: each schedule entry belongs to a user (staff/trainer) ---
            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.User)
                .WithMany(u => u.Schedules)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Class: each class is taught by one trainer (User) ---
            modelBuilder.Entity<Class>()
                .HasOne(c => c.User)
                .WithMany(u => u.Classes)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Room: each room belongs to one branch ---
            modelBuilder.Entity<Room>()
                .HasOne(r => r.Branch)
                .WithMany(b => b.Rooms)
                .HasForeignKey(r => r.BranchId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Session: links a Class to a Room at a specific time ---
            modelBuilder.Entity<Session>()
                .HasOne(s => s.Class)
                .WithMany(c => c.Sessions)
                .HasForeignKey(s => s.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.Room)
                .WithMany(r => r.Sessions)
                .HasForeignKey(s => s.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Booking: a member reserves a spot in a session ---
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Session)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Unique index: a user can only book the same session once.
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.SessionId, b.UserId })
                .IsUnique();

            // --- Attendance: tracks when a member checks in and out ---
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.User)
                .WithMany(u => u.Attendances)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Precision for money columns ---
            // 18 digits total, 2 after the decimal (e.g. 123456789012345678.99).
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Tier>()
                .Property(t => t.Price)
                .HasPrecision(18, 2);
        }
    }
}
