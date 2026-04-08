// ============================================================
// Session.cs — Represents a scheduled occurrence of a class.
// A session links a Class to a Room at a specific time window.
// Members book sessions to attend classes. The MaxCapacity
// limits how many bookings a session can accept.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Session
    {
        public int SessionId { get; set; }          // Auto-generated primary key
        public DateTime Start { get; set; }         // Session start time
        public DateTime End { get; set; }           // Session end time

        [Range(1, 100)]                             // Maximum attendees
        public required int MaxCapacity { get; set; }

        // Foreign keys — which class and which room
        public required int ClassId { get; set; }
        public required Class Class { get; set; }
        public required int RoomId { get; set; }
        public required Room Room { get; set; }

        // All bookings for this session
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
