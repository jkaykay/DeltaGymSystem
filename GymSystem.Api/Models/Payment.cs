namespace GymSystem.Api.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }

        //FK
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }


        public required int SubId { get; set; }
        public required Subscription Subscription { get; set; }
    }
}
