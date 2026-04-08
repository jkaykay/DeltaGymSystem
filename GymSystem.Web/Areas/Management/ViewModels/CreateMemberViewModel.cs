using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class CreateMemberViewModel
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

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

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [RegularExpression(@"^0?7\d{9}$", ErrorMessage = "Enter a valid UK mobile number (e.g. 7911123456).")]
        [MaxLength(15)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }
}