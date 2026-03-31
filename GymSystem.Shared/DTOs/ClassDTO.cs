using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public class ClassDTO
{
    public int ClassId { get; set; }

    [Required, MaxLength(100)]
    public string Subject { get; set; } = string.Empty;

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(201)]
    public string TrainerName { get; set; } = string.Empty;

    public int SessionCount { get; set; }
}
