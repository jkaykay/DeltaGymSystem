using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record AddScheduleRequest(
    [Required] DateTime Start,
    [Required] DateTime End,
    [Required, MaxLength(450)] string UserId
);
