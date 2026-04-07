using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Member.ViewModels
{
    public class ProfileViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [RegularExpression(@"^0?7\d{9}$", ErrorMessage = "Enter a valid UK mobile number (e.g. 7911123456).")]
        [Display(Name = "Phone Number")]
        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Join Date")]
        public DateTime? JoinDate { get; set; }

        public bool Active { get; set; }

        public string? QrCodeBase64 { get; set; }
        public DateTime? QrExpiresAt { get; set; }
    }
}