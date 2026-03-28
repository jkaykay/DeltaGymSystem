namespace GymSystem.Shared.DTOs;

public record ScanResponse(
    string Action,
    string MemberId,
    string MemberName,
    DateTime? CheckIn,
    DateTime? CheckOut
);