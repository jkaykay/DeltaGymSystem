using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [PersonalData]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [PersonalData]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public DateTime? JoinDate { get; set; } = DateTime.UtcNow;

        public bool Active { get; set; } = true;

        public DateTime? HireDate { get; set; }

        [MaxLength(50)]
        public string? EmployeeId { get; set; } // Internal staff ID

        public string? CreatedByUserId { get; set; }

        //FK
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }

        public ICollection<Subscription>? Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<Payment>? Payments { get; set; } = new List<Payment>();
        public ICollection<Class>? Classes { get; set; } = new List<Class>();
        public ICollection<Booking>? Bookings { get; set; } = new List<Booking>();
        public ICollection<Attendance>? Attendances { get; set; } = new List<Attendance>();

        public ICollection<Schedule>? Schedules { get; set; } = new List<Schedule>();
    }
}
