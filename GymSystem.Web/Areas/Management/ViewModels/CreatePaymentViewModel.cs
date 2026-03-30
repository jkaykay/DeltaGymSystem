using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class CreatePaymentViewModel
    {
        // Populated via JS from the selected subscription — not shown as an input
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Subscription")]
        public int SubId { get; set; }

        [Required]
        [Range(0.01, 99999.99, ErrorMessage = "Amount must match the tier price.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
    }
}