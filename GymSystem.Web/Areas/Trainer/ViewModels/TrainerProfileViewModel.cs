using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Trainer.ViewModels
{
    public class TrainerProfileViewModel
    {
        public string FullName { get; set; } = "";
        public string RoleLabel { get; set; } = "";
        public string UserName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [RegularExpression(@"^0?7\d{9}$", ErrorMessage = "Enter a valid UK mobile number (e.g. 7911123456).")]
        [Display(Name = "Phone Number")]
        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        public string GymLocation { get; set; } = "";
        public bool IsEditing { get; set; }
    }
}