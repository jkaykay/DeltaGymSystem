using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class CreateSubscriptionViewModel
    {
        [Required]
        [Display(Name = "Member")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(25)]
        [Display(Name = "Tier")]
        public string TierName { get; set; } = string.Empty;
    }
}