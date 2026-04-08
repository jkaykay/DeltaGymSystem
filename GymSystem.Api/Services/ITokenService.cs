// ============================================================
// ITokenService.cs — Interface for the JWT token generation service.
// An interface defines a "contract" — it says WHAT a service can do
// without saying HOW. This allows us to swap implementations easily
// (e.g. for testing). The actual logic lives in TokenService.cs.
// ============================================================

using GymSystem.Api.Models;

namespace GymSystem.Api.Services
{
    public interface ITokenService
    {
        // Given a user, create a signed JWT token string for authentication.
        Task<string> GenerateTokenAsync(ApplicationUser user);
    }
}
