using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class CreateScheduleViewModel
    {
        [Required]
        [Display(Name = "Branch")]
        public int BranchId { get; set; }

        [Required]
        [Display(Name = "Employee")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Start Time")]
        public DateTime Start { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "End Time")]
        public DateTime End { get; set; } = DateTime.Now;
    }
}
