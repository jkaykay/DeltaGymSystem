namespace GymSystem.Shared.DTOs;

// Result DTO containing a generated QR token and its expiration.
// Token: The generated QR token string.
// ExpiresAt: Date and time when the token expires.
public record QRTokenResult(string Token, DateTime ExpiresAt);
