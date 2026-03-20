using System;
using System.Collections.Generic;
using System.Text;

namespace GymSystem.Shared.DTOs
{
    public class SessionDTO
    {
        public int SessionId { get; set; }
        public DateTime Start { get; set; } 
        public DateTime End { get; set; }
        public string Subject { get; set; } = string.Empty;
        public int RoomNumber { get; set; }
        public int MaxCapacity { get; set; }
        public int BookingCount { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public int AvailableSpots => MaxCapacity - BookingCount;
    }
}
