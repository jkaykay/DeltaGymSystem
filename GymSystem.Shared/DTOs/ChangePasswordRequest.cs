using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(6), MaxLength(100)] string NewPassword
);