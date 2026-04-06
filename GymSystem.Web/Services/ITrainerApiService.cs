using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services
{
    public interface ITrainerApiService
    {
        Task<UserDTO?> GetTrainerProfileAsync(string token, CancellationToken cancellationToken = default);

        Task<bool> UpdateTrainerProfileAsync(UpdateTrainerProfileRequest request, string token, CancellationToken cancellationToken = default);

        Task<List<SessionDTO>> GetSessionsAsync(string token, CancellationToken cancellationToken = default);

        Task<SessionDTO?> GetSessionByIdAsync(int sessionId, string token, CancellationToken cancellationToken = default);

        Task<List<BookingDTO>> GetSessionBookingsAsync(int sessionId, string token, CancellationToken cancellationToken = default);

        Task<bool> DeleteSessionAsync(int sessionId, string token, CancellationToken cancellationToken = default);

        Task<List<RoomDTO>> GetRoomsByBranchAsync(int branchId, string token, CancellationToken cancellationToken = default);

        Task<List<ClassDTO>> GetTrainerClassesAsync(string trainerId, string token, CancellationToken cancellationToken = default);

        Task<bool> CreateSessionAsync(AddSessionRequest request, string token, CancellationToken cancellationToken = default);

    }
}