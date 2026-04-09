using GymSystem.Shared.DTOs;
using GymSystem.Web.Areas.Trainer.Models;

namespace GymSystem.Web.Services
{
    // Interface for the Trainer API service.
    // Defines operations available to trainers: viewing/updating their profile,
    // listing their sessions (with paging and search), viewing bookings for a session,
    // cancelling sessions, fetching rooms by branch, fetching their assigned classes,
    // and creating new training sessions.
    // Each method requires a JWT token because the trainer must be authenticated.
    public interface ITrainerApiService
    {
        // Fetches the currently logged-in trainer's profile from the API.
        Task<UserDTO?> GetTrainerProfileAsync(string token, CancellationToken cancellationToken = default);

        // Updates the trainer's own profile (email, phone, etc.).
        Task<(bool Success, string? Error)> UpdateTrainerProfileAsync(UpdateTrainerProfileRequest request, string token, CancellationToken cancellationToken = default);

        // Gets a paged list of sessions for a specific trainer (instructor).
        Task<TrainerPagedResult<SessionDTO>> GetSessionsAsync(string instructorId, int page, int pageSize, string token, string? search = null, CancellationToken cancellationToken = default);

        // Gets a single session by its ID.
        Task<SessionDTO?> GetSessionByIdAsync(int sessionId, string token, CancellationToken cancellationToken = default);

        // Gets all bookings (members signed up) for a specific session.
        Task<List<BookingDTO>> GetSessionBookingsAsync(int sessionId, string token, CancellationToken cancellationToken = default);

        // Deletes (cancels) a session by its ID.
        Task<bool> DeleteSessionAsync(int sessionId, string token, CancellationToken cancellationToken = default);

        // Gets all rooms belonging to a specific branch (for session creation dropdowns).
        Task<List<RoomDTO>> GetRoomsByBranchAsync(int branchId, string token, CancellationToken cancellationToken = default);

        // Gets the classes assigned to a specific trainer (for session creation dropdowns).
        Task<List<ClassDTO>> GetTrainerClassesAsync(string trainerId, string token, CancellationToken cancellationToken = default);

        // Gets a paged list of sessions for a trainer, filtered by optional date range.
        Task<TrainerPagedResult<SessionDTO>> GetSessionsByTrainerAsync(string instructorId, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize, string token, CancellationToken cancellationToken = default);

        // Creates a new training session via the API.
        Task<(bool Success, string? Error)> CreateSessionAsync(AddSessionRequest request, string token, CancellationToken cancellationToken = default);

    }
}
