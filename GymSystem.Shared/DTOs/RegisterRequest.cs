using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record RegisterRequest(
    [Required, MaxLength(100)] string UserName,
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password,
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName
);