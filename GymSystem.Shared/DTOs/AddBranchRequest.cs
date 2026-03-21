namespace GymSystem.Shared.DTOs;

public record AddBranchRequest(
    string Address,
    string City,
    string Province,
    string PostCode
);

