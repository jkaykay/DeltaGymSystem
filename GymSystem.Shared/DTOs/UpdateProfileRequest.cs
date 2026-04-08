using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs
{
    // Request DTO for a user to update their own profile email.
    // Email: New email address (required, must be a valid email format).
    public record UpdateProfileRequest(
        [Required, EmailAddress] string Email
    );
}
