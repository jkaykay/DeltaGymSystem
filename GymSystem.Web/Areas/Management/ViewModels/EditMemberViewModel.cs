using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class EditMemberViewModel
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

        [Required]
        [Display(Name = "Active")]
        public bool Active { get; set; }

        [RegularExpression(@"^0?7\d{9}$", ErrorMessage = "Enter a valid UK mobile number (e.g. 7911123456).")]
        [MaxLength(15)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }
}