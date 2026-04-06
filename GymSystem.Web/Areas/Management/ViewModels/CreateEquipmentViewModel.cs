using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class CreateEquipmentViewModel
    {
        [MaxLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Date Acquired")]
        public DateTime InDate { get; set; } = DateTime.Today;

        [Display(Name = "Operational")]
        public bool Operational { get; set; } = true;

        [Required]
        [Display(Name = "Room")]
        public int RoomId { get; set; }
    }
}
