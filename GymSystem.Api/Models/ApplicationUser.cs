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
    }
}
