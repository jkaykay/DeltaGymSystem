// ============================================================
// IQRTokenService.cs — Interface for QR code token operations.
// Defines the contract for generating time-limited, signed
// tokens that are embedded in QR codes for member check-in.
// ============================================================

using GymSystem.Shared.DTOs;

namespace GymSystem.Api.Services;

public interface IQRTokenService
{
    // Generate a signed, time-limited token for a member's QR code.
    QRTokenResult GenerateToken(string memberId);

    // Validate and decode a scanned QR token. Returns null if invalid or expired.
    QRTokenPayload? ValidateToken(string token);
}