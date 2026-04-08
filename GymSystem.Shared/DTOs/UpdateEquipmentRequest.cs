using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for partially updating equipment details. Null properties are left unchanged.
// Description: Updated description (max 500 characters, null to keep current).
// InDate: Updated installation date (null to keep current).
// Operational: Updated operational status (null to keep current).
// RoomId: Updated room assignment (null to keep current).
public record UpdateEquipmentRequest(
    [MaxLength(500)] string? Description = null,
    DateTime? InDate = null,
    bool? Operational = null,
    int? RoomId = null
);
