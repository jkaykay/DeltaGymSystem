using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for changing an authenticated user's password.
// CurrentPassword: The user's current password (required).
// NewPassword: The new password to set (required, 6–100 characters).
public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(6), MaxLength(100)] string NewPassword
);
