using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class EditSessionViewModel
    {
        public int SessionId { get; set; }

        // Read-only display — class cannot be changed after creation
        public int ClassId { get; set; }
        public string Subject { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Room")]
        public int RoomId { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        public DateTime Start { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public DateTime End { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Capacity must be between 1 and 100.")]
        [Display(Name = "Max Capacity")]
        public int MaxCapacity { get; set; }
    }
}