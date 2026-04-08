using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for partially updating a schedule entry. Null properties are left unchanged.
// Start: Updated start date and time (null to keep current).
// End: Updated end date and time (null to keep current).
// UserId: Updated assigned user identifier (max 450 characters, null to keep current).
public record UpdateScheduleRequest(
    DateTime? Start = null,
    DateTime? End = null,
    [MaxLength(450)] string? UserId = null
);
