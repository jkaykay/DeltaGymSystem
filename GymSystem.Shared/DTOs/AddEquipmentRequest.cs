using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record AddEquipmentRequest(
    [MaxLength(500)] string? Description,
    [Required] DateTime InDate,
    [Required] bool Operational,
    [Required] int RoomId
);
