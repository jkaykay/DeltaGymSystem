using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for adding a new piece of equipment to a room.
// Description: Optional description of the equipment (max 500 characters).
// InDate: Date the equipment was installed or acquired (required).
// Operational: Whether the equipment is operational (required).
// RoomId: Foreign key to the room (required).
public record AddEquipmentRequest(
    [MaxLength(500)] string? Description,
    [Required] DateTime InDate,
    [Required] bool Operational,
    [Required] int RoomId
);
