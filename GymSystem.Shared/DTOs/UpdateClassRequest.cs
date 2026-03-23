using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record UpdateClassRequest(
    [MaxLength(100)] string? Subject = null,
    string? UserId = null
);