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

        [Phone]
        [Display(Name = "Phone Number")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public string GymLocation { get; set; } = "";
        public bool IsEditing { get; set; }
    }
}