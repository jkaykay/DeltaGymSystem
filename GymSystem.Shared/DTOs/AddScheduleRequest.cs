using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for creating a new work schedule entry.
// Start: Schedule start date and time (required).
// End: Schedule end date and time (required).
// UserId: Identity identifier of the assigned user (required, max 450 characters).
public record AddScheduleRequest(
    [Required] DateTime Start,
    [Required] DateTime End,
    [Required, MaxLength(450)] string UserId
);
