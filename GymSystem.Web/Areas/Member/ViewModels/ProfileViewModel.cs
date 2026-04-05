using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Member.ViewModels
{
    public class ProfileViewModel
    {
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Telephone")]
        public string? Telephone { get; set; }

        [Display (Name = "Emergency Contact Name")]
        public string? EmergencyContact { get; set; }

        [Display(Name = "Weight")]
        public double? Weight { get; set; } 

        public string? MembershipName { get; set; } = string.Empty;
        public double? MembershipPrice { get; set; }

        public string? MemberCode { get; set; } = string.Empty;

    


    }
}