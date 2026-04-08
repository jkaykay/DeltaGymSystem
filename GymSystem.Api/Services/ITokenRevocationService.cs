// ============================================================
// ITokenRevocationService.cs — Interface for token blacklisting.
// JWT tokens are stateless — once issued, they're valid until
// they expire. To support "logout", we blacklist the token's
// unique ID (JTI) so the server rejects it on future requests.
// ============================================================

namespace GymSystem.Api.Services
{
    public interface ITokenRevocationService
    {
        // Add a token's JTI to the blacklist. The entry auto-expires
        // when the token itself would have expired.
        void Revoke(string jti, DateTime tokenExpiry);

        // Check whether a token has been revoked (i.e. user logged out).
        bool IsRevoked(string jti);
    }
}
