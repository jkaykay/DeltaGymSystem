using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record UpdateEquipmentRequest(
    [MaxLength(500)] string? Description = null,
    DateTime? InDate = null,
    bool? Operational = null,
    int? RoomId = null
);
