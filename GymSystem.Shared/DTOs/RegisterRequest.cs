using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for self-registration of a new member account.
// UserName: Desired username (required, max 100 characters).
// Email: Email address (required, validated format, max 256 characters).
// Password: Account password (required, 6–100 characters).
// FirstName: First name (required, max 100 characters).
// LastName: Last name (required, max 100 characters).
public record RegisterRequest(
    [Required, MaxLength(100)] string UserName,
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password,
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName
);
