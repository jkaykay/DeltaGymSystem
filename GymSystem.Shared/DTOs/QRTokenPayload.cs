namespace GymSystem.Shared.DTOs;

// Decoded payload from a QR token, containing the member identity and expiry.
// MemberId: Identity identifier of the member encoded in the QR token.
// ExpiresAt: Date and time when the QR token expires.
public record QRTokenPayload(string MemberId, DateTime ExpiresAt);
