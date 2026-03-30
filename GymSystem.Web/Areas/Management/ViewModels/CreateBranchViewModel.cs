using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class CreateBranchViewModel
    {
        [Required]
        [MaxLength(200)]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Province")]
        public string Province { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        [Display(Name = "Post Code")]
        public string PostCode { get; set; } = string.Empty;

        [Display(Name = "Opening Date")]
        [DataType(DataType.Date)]
        public DateTime? OpenDate { get; set; }
    }
}