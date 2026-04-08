using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Data transfer object representing a gym class (e.g. Yoga, Boxing).
public class ClassDTO
{
    // Unique class identifier.
    public int ClassId { get; set; }

    // Subject or name of the class (required, max 100 characters).
    [Required, MaxLength(100)]
    public string Subject { get; set; } = string.Empty;

    // Identity identifier of the trainer who runs this class (required, max 450 characters).
    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    // Display name of the trainer (max 201 characters).
    [MaxLength(201)]
    public string TrainerName { get; set; } = string.Empty;

    // Total number of sessions scheduled for this class.
    public int SessionCount { get; set; }

    // Number of upcoming (future) sessions for this class.
    public int UpcomingSessionCount { get; set; }
}
