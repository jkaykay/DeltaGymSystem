using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Session
    {
        public int SessionId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        [Range(1, 100)]
        public required int MaxCapacity { get; set; }

        //FKs
        public required int ClassId { get; set; }
        public required Class Class { get; set; }
        public required int RoomId { get; set; }
        public required Room Room { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
