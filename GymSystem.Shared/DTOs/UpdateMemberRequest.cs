using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record UpdateMemberRequest(
    [EmailAddress, MaxLength(256)] string? Email = null,
    [MaxLength(100)] string? FirstName = null,
    [MaxLength(100)] string? LastName = null,
    [MaxLength(256)] string? QrCodeBase64 = null,
    bool? IsActive = null,
    [Phone, MaxLength(20)] string? PhoneNumber = null
);