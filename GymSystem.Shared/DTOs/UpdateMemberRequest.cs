using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record UpdateMemberRequest(
    [MaxLength(100)] string? FirstName = null,
    [MaxLength(100)] string? LastName = null,
    bool? Active = null
);