// ============================================================
// Class.cs — Represents a gym class (e.g. "Yoga", "Pilates").
// Each class has a subject name and is taught by one trainer.
// A class can have multiple scheduled sessions.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Class
    {
        public int ClassId { get; set; }            // Auto-generated primary key

        [Required]
        [MaxLength(100)]
        public string Subject { get; set; } = string.Empty; // e.g. "Yoga", "HIIT"

        // Foreign key to the Trainer who teaches this class
        [MaxLength(450)]
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }

        // All scheduled sessions for this class
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
