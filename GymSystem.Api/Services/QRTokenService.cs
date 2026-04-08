// ============================================================
// QRTokenService.cs — Generates and validates QR code tokens.
// Tokens are HMAC-signed strings containing the member's ID and
// an expiry timestamp. Staff scan the QR code to check members
// in/out. The signature prevents tampering.
// ============================================================

using System.Security.Cryptography;
using System.Text;
using GymSystem.Shared.DTOs;


namespace GymSystem.Api.Services;

public class QRTokenService : IQRTokenService
{
    private readonly byte[] _keyBytes;      // Secret key used for HMAC signing
    private readonly int _expiryMinutes;    // How long a QR token stays valid

    // Read the signing secret and expiry from appsettings.json.
    public QRTokenService(IConfiguration configuration)
    {
        var secret = configuration["QrCode:Secret"]
            ?? throw new InvalidOperationException("QrCode:Secret is not configured.");

        _keyBytes = Encoding.UTF8.GetBytes(secret);
        _expiryMinutes = configuration.GetValue<int>("QrCode:ExpiryMinutes", 5);
    }

    // Creates a token string: "memberId|expiryTimestamp|signature",
    // then Base64-encodes it so it can be embedded in a QR code.
    public QRTokenResult GenerateToken(string memberId)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_expiryMinutes);
        var payload = $"{memberId}|{expiresAt.ToUnixTimeSeconds()}";
        var signature = Sign(payload);
        var raw = $"{payload}|{signature}";
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));

        return new QRTokenResult(token, expiresAt.UtcDateTime);
    }

    // Decodes and verifies a scanned token. Returns the member's ID
    // and expiry if valid; returns null if tampered with or expired.
    public QRTokenPayload? ValidateToken(string token)
    {
        try
        {
            // Decode from Base64 back to the raw "memberId|expiry|signature" string
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(token));

            // The signature is everything after the last pipe
            var lastPipe = raw.LastIndexOf('|');
            if (lastPipe < 0) return null;

            var payload = raw[..lastPipe];
            var providedSig = raw[(lastPipe + 1)..];
            var expectedSig = Sign(payload);

            // Constant-time comparison to prevent timing attacks
            if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(providedSig),
                Encoding.UTF8.GetBytes(expectedSig)))
                return null;

            // Parse the payload parts
            var parts = payload.Split('|');
            if (parts.Length != 2) return null;

            var memberId = parts[0];
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(parts[1])).UtcDateTime;

            // Reject expired tokens
            if (DateTime.UtcNow > expiresAt) return null;

            return new QRTokenPayload(memberId, expiresAt);
        }
        catch
        {
            // Any parsing error means the token is invalid
            return null;
        }
    }

    // Computes an HMAC-SHA256 signature of the data using the secret key.
    // Returns a hex string (fixed length of 64 chars).
    private string Sign(string data)
    {
        using var hmac = new HMACSHA256(_keyBytes);
        // Hex string is used — fixed length (64 chars) regardless of input
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)));
    }
}