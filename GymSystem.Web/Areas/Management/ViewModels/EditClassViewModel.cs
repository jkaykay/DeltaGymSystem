using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class EditClassViewModel
    {
        public int ClassId { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Subject")]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Trainer")]
        public string UserId { get; set; } = string.Empty;
    }
}