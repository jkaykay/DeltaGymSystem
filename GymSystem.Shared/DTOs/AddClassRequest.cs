using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record AddClassRequest(
    [Required, MaxLength(100)] string Subject,
    [Required] string UserId
);