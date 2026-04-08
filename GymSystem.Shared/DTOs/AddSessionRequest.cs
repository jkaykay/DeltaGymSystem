using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for creating a new class session.
// Start: Session start date and time (required).
// End: Session end date and time (required).
// RoomId: Foreign key to the room (required).
// ClassId: Foreign key to the class (required).
// MaxCapacity: Maximum number of bookings allowed (required, 1–100).
public record AddSessionRequest(
    [Required] DateTime Start,
    [Required] DateTime End,
    [Required] int RoomId,
    [Required] int ClassId,
    [Required, Range(1, 100)] int MaxCapacity
);
