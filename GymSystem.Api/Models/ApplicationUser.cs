// ============================================================
// ApplicationUser.cs — The user model for the application.
// Extends IdentityUser (which provides Id, Email, PasswordHash, etc.)
// with gym-specific fields like FirstName, HireDate, and BranchId.
// This single model represents all user types: Members, Staff,
// Trainers, and Admins. The user's role determines their type.
// ============================================================

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Personal details
        [Required]
        [PersonalData]              // Marks data as personal (for GDPR compliance)
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [PersonalData]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        // When the user joined the gym (applies to all user types)
        public DateTime? JoinDate { get; set; } = DateTime.UtcNow;

        // Whether the user's account is active (inactive members cannot check in)
        public bool Active { get; set; } = true;

        // Staff/Trainer-specific fields
        public DateTime? HireDate { get; set; }

        [MaxLength(50)]
        public string? EmployeeId { get; set; } // Internal staff ID

        // Foreign key to the Branch this user belongs to (optional)
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }      // Navigation property to the related Branch

        // Navigation collections — EF Core uses these to load related data.
        // For example, user.Subscriptions returns all subscriptions for this user.
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Class> Classes { get; set; } = new List<Class>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}