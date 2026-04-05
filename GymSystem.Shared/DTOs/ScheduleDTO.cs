using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public class ScheduleDTO
{
    public int ScheduleId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(201)]
    public string UserName { get; set; } = string.Empty;
}
