using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for an admin or staff to create a new member account.
// UserName: Username for the new member (required, max 100 characters).
// Email: Email address (required, validated format, max 256 characters).
// FirstName: First name (required, max 100 characters).
// LastName: Last name (required, max 100 characters).
// Password: Initial password (required, 6–100 characters).
// PhoneNumber: Optional phone number (max 20 characters).
public record CreateMemberRequest(
    [Required, MaxLength(100)] string UserName,
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, MinLength(6), MaxLength(100)] string Password,
    [Phone, MaxLength(20)] string? PhoneNumber = null
);
