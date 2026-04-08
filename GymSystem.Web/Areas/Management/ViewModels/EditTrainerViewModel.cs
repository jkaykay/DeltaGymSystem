using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class EditTrainerViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(50)]
        [Display(Name = "Employee ID")]
        public string? EmployeeId { get; set; }

        [Display(Name = "Branch")]
        public int? BranchId { get; set; }

        [RegularExpression(@"^0?7\d{9}$", ErrorMessage = "Enter a valid UK mobile number (e.g. 7911123456).")]
        [MaxLength(15)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public List<GymSystem.Shared.DTOs.BranchDTO> Branches { get; set; } = [];
    }
}