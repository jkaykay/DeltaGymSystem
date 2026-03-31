using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class EditTierViewModel
    {
        // Hidden — immutable PK, used to identify the record
        public string TierName { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 99999.99, ErrorMessage = "Price must be between 0.01 and 99999.99.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Monthly Price")]
        public decimal Price { get; set; }
    }
}