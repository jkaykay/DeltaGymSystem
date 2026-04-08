using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Data transfer object representing a piece of gym equipment.
public class EquipmentDTO
{
    // Unique equipment identifier.
    public int EquipmentId { get; set; }

    // Optional description of the equipment (max 500 characters).
    [MaxLength(500)]
    public string? Description { get; set; }

    // Date the equipment was installed or acquired.
    public DateTime InDate { get; set; }

    // Whether the equipment is currently operational.
    public bool Operational { get; set; }

    // Foreign key to the room where the equipment is located.
    public int RoomId { get; set; }

    // Room number where the equipment is located (resolved server-side).
    public int RoomNumber { get; set; }
}
