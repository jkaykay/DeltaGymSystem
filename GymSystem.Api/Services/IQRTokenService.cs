using GymSystem.Shared.DTOs;

namespace GymSystem.Api.Services;

public interface IQRTokenService
{
    QRTokenResult GenerateToken(string memberId);
    QRTokenPayload? ValidateToken(string token);
}