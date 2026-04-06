using GymSystem.Shared.DTOs;
using GymSystem.Web.Areas.Trainer.Models;

namespace GymSystem.Web.Services
{
    public interface ITrainerApiService
    {
        Task<UserDTO?> GetTrainerProfileAsync(string token, CancellationToken cancellationToken = default);

        Task<bool> UpdateTrainerProfileAsync(UpdateTrainerProfileRequest request, string token, CancellationToken cancellationToken = default);

        Task<TrainerPagedResult<SessionDTO>> GetSessionsAsync(string instructorId, int page, int pageSize, string token, string? search = null, CancellationToken cancellationToken = default);

        Task<SessionDTO?> GetSessionByIdAsync(int sessionId, string token, CancellationToken cancellationToken = default);

        Task<List<BookingDTO>> GetSessionBookingsAsync(int sessionId, string token, CancellationToken cancellationToken = default);

        Task<bool> DeleteSessionAsync(int sessionId, string token, CancellationToken cancellationToken = default);

        Task<List<RoomDTO>> GetRoomsByBranchAsync(int branchId, string token, CancellationToken cancellationToken = default);

        Task<List<ClassDTO>> GetTrainerClassesAsync(string trainerId, string token, CancellationToken cancellationToken = default);

        Task<TrainerPagedResult<SessionDTO>> GetSessionsByTrainerAsync(string instructorId, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize, string token, CancellationToken cancellationToken = default);

        Task<bool> CreateSessionAsync(AddSessionRequest request, string token, CancellationToken cancellationToken = default);

    }
}