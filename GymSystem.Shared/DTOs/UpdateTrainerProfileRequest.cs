using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record UpdateTrainerProfileRequest(
    [EmailAddress, MaxLength(256)] string? Email = null,
    [MaxLength(100)] string? FirstName = null,
    [MaxLength(100)] string? LastName = null,
    [Phone, MaxLength(20)] string? PhoneNumber = null
);