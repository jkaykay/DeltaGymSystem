using System.Security.Cryptography;
using System.Text;
using GymSystem.Shared.DTOs;


namespace GymSystem.Api.Services;

public class QRTokenService : IQRTokenService
{
    private readonly byte[] _keyBytes;
    private readonly int _expiryMinutes;

    public QRTokenService(IConfiguration configuration)
    {
        var secret = configuration["QrCode:Secret"]
            ?? throw new InvalidOperationException("QrCode:Secret is not configured.");

        _keyBytes = Encoding.UTF8.GetBytes(secret);
        _expiryMinutes = configuration.GetValue<int>("QrCode:ExpiryMinutes", 5);
    }

    public QRTokenResult GenerateToken(string memberId)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_expiryMinutes);
        var payload = $"{memberId}|{expiresAt.ToUnixTimeSeconds()}";
        var signature = Sign(payload);
        var raw = $"{payload}|{signature}";
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));

        return new QRTokenResult(token, expiresAt.UtcDateTime);
    }

    public QRTokenPayload? ValidateToken(string token)
    {
        try
        {
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

            var parts = payload.Split('|');
            if (parts.Length != 2) return null;

            var memberId = parts[0];
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(parts[1])).UtcDateTime;

            if (DateTime.UtcNow > expiresAt) return null;

            return new QRTokenPayload(memberId, expiresAt);
        }
        catch
        {
            return null;
        }
    }

    private string Sign(string data)
    {
        using var hmac = new HMACSHA256(_keyBytes);
        // Hex string is used — fixed length (64 chars) regardless of input
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)));
    }
}