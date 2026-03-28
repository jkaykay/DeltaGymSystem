namespace GymSystem.Api.Services
{
    public interface ITokenRevocationService
    {
        void Revoke(string jti, DateTime tokenExpiry);
        bool IsRevoked(string jti);
    }
}
