using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class EditRoomViewModel
    {
        public int RoomId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Room number must be between 1 and 100.")]
        [Display(Name = "Room Number")]
        public int RoomNumber { get; set; }

        [Required]
        [Display(Name = "Branch")]
        public int BranchId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Capacity must be between 1 and 100.")]
        [Display(Name = "Max Capacity")]
        public int MaxCapacity { get; set; }
    }
}