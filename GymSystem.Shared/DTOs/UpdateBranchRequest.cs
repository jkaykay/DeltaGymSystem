namespace GymSystem.Shared.DTOs;

public record UpdateBranchRequest(
    string? Address = null,
    string? City = null,
    string? Province = null,
    string? PostCode = null
);