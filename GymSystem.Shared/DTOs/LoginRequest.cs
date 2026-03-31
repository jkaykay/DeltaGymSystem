using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record LoginRequest(
    [Required, MaxLength(256)] string EmailOrUserName,
    [Required] string Password
);