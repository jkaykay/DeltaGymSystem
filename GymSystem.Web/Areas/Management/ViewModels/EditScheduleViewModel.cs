using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class EditScheduleViewModel
    {
        public int ScheduleId { get; set; }

        // Read-only display — employee cannot be changed after creation
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Start Time")]
        public DateTime Start { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public DateTime End { get; set; }
    }
}
