using Microsoft.Extensions.Caching.Memory;

namespace GymSystem.Api.Services
{
    public class TokenRevocationService : ITokenRevocationService
    {
        private readonly IMemoryCache _cache;

        public TokenRevocationService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Revoke(string jti, DateTime tokenExpiry)
        {
            // Cache entry expires exactly when the token itself would have expired —
            // the blacklist self-cleans with no manual housekeeping required.
            _cache.Set($"jti_revoked:{jti}", true, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = new DateTimeOffset(tokenExpiry, TimeSpan.Zero)
            });
        }

        public bool IsRevoked(string jti) =>
            _cache.TryGetValue($"jti_revoked:{jti}", out _);
    }
}
