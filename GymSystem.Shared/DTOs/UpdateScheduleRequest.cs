using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record UpdateScheduleRequest(
    DateTime? Start = null,
    DateTime? End = null,
    [MaxLength(450)] string? UserId = null
);
