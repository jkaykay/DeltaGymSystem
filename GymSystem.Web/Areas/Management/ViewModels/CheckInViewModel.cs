using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class CheckInViewModel
    {
        [Required]
        [Display(Name = "Member")]
        public string UserId { get; set; } = string.Empty;
    }
}