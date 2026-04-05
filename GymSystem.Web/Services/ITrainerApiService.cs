using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services
{
    public interface ITrainerApiService
    {
        Task<UserDTO?> GetTrainerProfileAsync(string token, CancellationToken cancellationToken = default);

        Task<bool> UpdateTrainerProfileAsync(UpdateTrainerProfileRequest request, string token, CancellationToken cancellationToken = default);

        Task<List<SessionDTO>> GetSessionsAsync(string token, CancellationToken cancellationToken = default);

    }
}