using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record UpdateBranchRequest(
    [MaxLength(200)] string? Address = null,
    [MaxLength(100)] string? City = null,
    [MaxLength(100)] string? Province = null,
    [MaxLength(20)] string? PostCode = null
);