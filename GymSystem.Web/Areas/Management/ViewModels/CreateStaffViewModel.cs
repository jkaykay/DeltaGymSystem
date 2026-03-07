using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class CreateStaffViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Staff"; // "Staff" or "Admin"

        [MaxLength(50)]
        [Display(Name = "Employee ID (e.g., GYM-2026-001)")]
        public string? EmployeeId { get; set; }
    }
}