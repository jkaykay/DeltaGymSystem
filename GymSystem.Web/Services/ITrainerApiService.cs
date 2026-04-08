using GymSystem.Shared.DTOs;
using GymSystem.Web.Areas.Trainer.Models;

namespace GymSystem.Web.Services
{
    /// <summary>
    /// Interface for the Trainer API service.
    /// Defines operations available to trainers: viewing/updating their profile,
    /// listing their sessions (with paging and search), viewing bookings for a session,
    /// cancelling sessions, fetching rooms by branch, fetching their assigned classes,
    /// and creating new training sessions.
    /// Each method requires a JWT token because the trainer must be authenticated.
    /// </summary>
    public interface ITrainerApiService
    {
        /// <summary>Fetches the currently logged-in trainer's profile from the API.</summary>
        Task<UserDTO?> GetTrainerProfileAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>Updates the trainer's own profile (email, phone, etc.).</summary>
        Task<bool> UpdateTrainerProfileAsync(UpdateTrainerProfileRequest request, string token, CancellationToken cancellationToken = default);

        /// <summary>Gets a paged list of sessions for a specific trainer (instructor).</summary>
        Task<TrainerPagedResult<SessionDTO>> GetSessionsAsync(string instructorId, int page, int pageSize, string token, string? search = null, CancellationToken cancellationToken = default);

        /// <summary>Gets a single session by its ID.</summary>
        Task<SessionDTO?> GetSessionByIdAsync(int sessionId, string token, CancellationToken cancellationToken = default);

        /// <summary>Gets all bookings (members signed up) for a specific session.</summary>
        Task<List<BookingDTO>> GetSessionBookingsAsync(int sessionId, string token, CancellationToken cancellationToken = default);

        /// <summary>Deletes (cancels) a session by its ID.</summary>
        Task<bool> DeleteSessionAsync(int sessionId, string token, CancellationToken cancellationToken = default);

        /// <summary>Gets all rooms belonging to a specific branch (for session creation dropdowns).</summary>
        Task<List<RoomDTO>> GetRoomsByBranchAsync(int branchId, string token, CancellationToken cancellationToken = default);

        /// <summary>Gets the classes assigned to a specific trainer (for session creation dropdowns).</summary>
        Task<List<ClassDTO>> GetTrainerClassesAsync(string trainerId, string token, CancellationToken cancellationToken = default);

        /// <summary>Gets a paged list of sessions for a trainer, filtered by optional date range.</summary>
        Task<TrainerPagedResult<SessionDTO>> GetSessionsByTrainerAsync(string instructorId, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize, string token, CancellationToken cancellationToken = default);

        /// <summary>Creates a new training session via the API.</summary>
        Task<bool> CreateSessionAsync(AddSessionRequest request, string token, CancellationToken cancellationToken = default);

    }
}