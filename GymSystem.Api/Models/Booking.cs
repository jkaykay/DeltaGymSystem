using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        
        public DateTime BookDate { get; set; }

        //FK
        public required int SessionId { get; set; }
        public required Session Session { get; set; }

        [MaxLength(450)]
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }

    }
}
