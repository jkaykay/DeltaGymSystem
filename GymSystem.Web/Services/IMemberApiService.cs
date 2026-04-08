using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services;

// Interface for the Member API service.
// Defines operations available to gym members: authentication (login/register),
// profile management, QR code retrieval, dashboard data (bookings, attendances,
// payments), session booking, subscription management, and an LLM chat feature.
// Also includes public (anonymous) endpoints for tiers, trainers, classes, and sessions.
public interface IMemberApiService
{
    // Auth
    Task<(bool Success, LoginResponse? Data, string? Error)> LoginAsync(LoginRequest request);
    Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest request);

    // Profile
    Task<UserDTO?> GetMyProfileAsync();
    Task<(bool Success, string? Error)> UpdateProfileAsync(string memberId, UpdateMemberRequest request);

    // QR
    Task<QRCodeResponse?> GetMyQRAsync(string memberId);

    // Dashboard — uses real API endpoints
    Task<PagedResult<BookingDTO>> GetMyBookingsAsync(int page = 1, int pageSize = 100, string? search = null);
    Task<List<AttendanceDTO>> GetMyAttendancesAsync(string memberId);
    Task<PagedResult<PaymentDTO>> GetMyPaymentsAsync(int page = 1, int pageSize = 100);

    // Booking
    Task<PagedResult<SessionDTO>> GetUpcomingSessionsAsync(int page = 1, int pageSize = 100, string? search = null);
    Task<(bool Success, string? Error)> CreateMyBookingAsync(int sessionId);
    Task<(bool Success, string? Error)> CancelMyBookingAsync(int bookingId);

    // Subscriptions
    Task<PagedResult<SubscriptionDTO>> GetMySubscriptionsAsync(int page = 1, int pageSize = 100);
    Task<(bool Success, SubscriptionDTO? Data, string? Error)> CreateMySubscriptionAsync(string tierName);
    Task<(bool Success, string? Error)> CreateMyPaymentAsync(int subId, decimal amount);

    // Public data (anonymous-safe)
    Task<List<TierDTO>> GetAllTiersAsync();
    Task<List<UserDTO>> GetRandomTrainersAsync(int count = 3);
    Task<PagedResult<ClassDTO>> GetClassesAsync(int page = 1, int pageSize = 100);
    Task<PagedResult<SessionDTO>> GetSessionsAsync(int page = 1, int pageSize = 100);

    // LLM chat
    Task<string?> AskDeltaAsync(string prompt);
}
