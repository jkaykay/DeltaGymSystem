using System;
using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs
{
    public class BookingDTO
    {
        public int BookingId { get; set; }
        public DateTime BookDate { get; set; }

        public int SessionId { get; set; }
        public DateTime SessionStart { get; set; }
        public DateTime SessionEnd { get; set; }

        [MaxLength(100)]
        public string Subject { get; set; } = string.Empty;

        public int RoomNumber { get; set; }

        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [MaxLength(201)]
        public string UserName { get; set; } = string.Empty;
    }
}