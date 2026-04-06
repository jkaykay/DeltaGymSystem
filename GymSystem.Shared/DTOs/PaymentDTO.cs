using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs
{
    public class PaymentDTO
    {
        public int PaymentId { get; set; }

        [Range(0.01, 99999.99)]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        [Required, MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        public string? UserFullName { get; set; }

        public int SubId { get; set; }
    }
}
