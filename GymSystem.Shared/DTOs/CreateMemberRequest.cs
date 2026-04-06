using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record CreateMemberRequest(
    [Required, MaxLength(100)] string UserName,
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, MinLength(6), MaxLength(100)] string Password,
    [Phone, MaxLength(20)] string? PhoneNumber = null
);