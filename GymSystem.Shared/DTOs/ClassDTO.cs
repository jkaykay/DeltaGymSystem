namespace GymSystem.Shared.DTOs;

public class ClassDTO
{
    public int ClassId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string TrainerName { get; set; } = string.Empty;
    public int SessionCount { get; set; }
}
