using GymSystem.Api.Models;

namespace GymSystem.Api.Services
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(ApplicationUser user);
    }
}
