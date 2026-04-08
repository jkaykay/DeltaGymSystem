using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for partially updating a session. Null properties are left unchanged.
// Start: Updated session start date and time (null to keep current).
// End: Updated session end date and time (null to keep current).
// RoomId: Updated room assignment (null to keep current).
// MaxCapacity: Updated maximum capacity (1–100, null to keep current).
public record UpdateSessionRequest(
    DateTime? Start = null,
    DateTime? End = null,
    int? RoomId = null,
    [Range(1, 100)] int? MaxCapacity = null
);
