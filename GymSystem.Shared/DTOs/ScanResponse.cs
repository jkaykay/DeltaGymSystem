namespace GymSystem.Shared.DTOs;

// Response DTO returned after processing a QR code scan.
// Action: The action taken (e.g. "CheckIn" or "CheckOut").
// MemberId: Identity identifier of the scanned member.
// MemberName: Display name of the scanned member.
// CheckIn: Check-in timestamp (null if action was check-out only).
// CheckOut: Check-out timestamp (null if action was check-in only).
public record ScanResponse(
    string Action,
    string MemberId,
    string MemberName,
    DateTime? CheckIn,
    DateTime? CheckOut
);
