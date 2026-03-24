using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record AddClassRequest(
    [Required, MaxLength(100)] string Subject,
    [Required, MaxLength(450)] string UserId
);