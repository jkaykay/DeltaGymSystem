using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record UpdateMemberRequest(
    [EmailAddress, MaxLength(256)] string? Email = null,
    [MaxLength(100)] string? FirstName = null,
    [MaxLength(100)] string? LastName = null,
    bool? Active = null
);