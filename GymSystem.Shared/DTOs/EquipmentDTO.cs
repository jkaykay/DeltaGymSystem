using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public class EquipmentDTO
{
    public int EquipmentId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime InDate { get; set; }

    public bool Operational { get; set; }

    public int RoomId { get; set; }

    public int RoomNumber { get; set; }
}
