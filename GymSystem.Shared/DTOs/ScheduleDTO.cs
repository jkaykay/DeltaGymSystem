using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Data transfer object representing a staff or trainer work schedule entry.
public class ScheduleDTO
{
    // Unique schedule entry identifier.
    public int ScheduleId { get; set; }

    // Schedule start date and time.
    public DateTime Start { get; set; }

    // Schedule end date and time.
    public DateTime End { get; set; }

    // Identity identifier of the assigned user (max 450 characters).
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    // Display name of the assigned user (max 201 characters).
    [MaxLength(201)]
    public string UserName { get; set; } = string.Empty;
}
