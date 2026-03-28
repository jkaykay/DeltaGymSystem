using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs
{
    public record UpdateProfileRequest(
        [Required, EmailAddress] string Email
    );
}