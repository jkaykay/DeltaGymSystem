using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class EditEquipmentViewModel
    {
        public int EquipmentId { get; set; }

        [MaxLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Date Acquired")]
        public DateTime InDate { get; set; }

        [Display(Name = "Operational")]
        public bool Operational { get; set; }

        [Required]
        [Display(Name = "Room")]
        public int RoomId { get; set; }
    }
}
