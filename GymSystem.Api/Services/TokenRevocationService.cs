// ============================================================
// TokenRevocationService.cs — In-memory token blacklist.
// Uses IMemoryCache to store revoked token IDs. Each entry
// automatically expires when the original token would have
// expired, so the blacklist cleans itself up with no manual work.
// Registered as a Singleton in Program.cs so all requests
// share the same blacklist.
// ============================================================

using Microsoft.Extensions.Caching.Memory;

namespace GymSystem.Api.Services
{
    public class TokenRevocationService : ITokenRevocationService
    {
        private readonly IMemoryCache _cache; // In-memory key-value store

        public TokenRevocationService(IMemoryCache cache)
        {
            _cache = cache;
        }

        // Blacklist a token by storing its JTI in the cache.
        public void Revoke(string jti, DateTime tokenExpiry)
        {
            // Cache entry expires exactly when the token itself would have expired —
            // the blacklist self-cleans with no manual housekeeping required.
            _cache.Set($"jti_revoked:{jti}", true, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = new DateTimeOffset(tokenExpiry, TimeSpan.Zero)
            });
        }

        // Returns true if the token's JTI is found in the blacklist.
        public bool IsRevoked(string jti) =>
            _cache.TryGetValue($"jti_revoked:{jti}", out _);
    }
}
