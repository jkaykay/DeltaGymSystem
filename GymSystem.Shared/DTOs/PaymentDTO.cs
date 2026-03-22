namespace GymSystem.Shared.DTOs
{
    public class PaymentDTO
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int SubId { get; set; }
    }
}
