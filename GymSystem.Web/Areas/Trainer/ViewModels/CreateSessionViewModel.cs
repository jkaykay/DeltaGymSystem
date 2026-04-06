using System.ComponentModel.DataAnnotations;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Areas.Trainer.ViewModels
{
    public class CreateSessionViewModel
    {
        [Required]
        [Display(Name = "Class")]
        public int ClassId { get; set; }

        [Required]
        [Display(Name = "Room")]
        public int RoomId { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        public DateTime Start { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "End Time")]
        public DateTime End { get; set; } = DateTime.Now;

        [Required]
        [Range(1, 100, ErrorMessage = "Capacity must be between 1 and 100.")]
        [Display(Name = "Max Capacity")]
        public int MaxCapacity { get; set; }

        public List<ClassDTO> Classes { get; set; } = [];

        public List<RoomDTO> Rooms { get; set; } = [];
    }
}
