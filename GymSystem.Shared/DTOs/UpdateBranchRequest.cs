namespace GymSystem.Shared.DTOs;

public record UpdateBranchRequest(
    string Address,
    string City,
    string Province,
    string PostCode
);