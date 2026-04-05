using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services;

public interface IMemberApiService
{
    // Auth
    Task<(bool Success, LoginResponse? Data, string? Error)> LoginAsync(LoginRequest request);
    Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest request);

    // Profile
    Task<ProfileDto?> GetMyProfileAsync();
    Task<(bool Success, string? Error)> UpdateProfileAsync(ProfileViewModel model);

    // QR
    Task<QRCodeResponse?> GetMyQRAsync(string memberId);

    // Dashboard — uses real API endpoints
    Task<PagedResult<BookingDTO>> GetMyBookingsAsync(int page = 1, int pageSize = 100);
    Task<List<AttendanceDTO>> GetMyAttendancesAsync(string memberId);
    Task<PagedResult<PaymentDTO>> GetMyPaymentsAsync(int page = 1, int pageSize = 100);

    // Booking
    Task<PagedResult<SessionDTO>> GetUpcomingSessionsAsync(int page = 1, int pageSize = 100);
    Task<(bool Success, string? Error)> CreateMyBookingAsync(int sessionId);
    Task<(bool Success, string? Error)> CancelMyBookingAsync(int bookingId);

    // Subscriptions
    Task<PagedResult<SubscriptionDTO>> GetMySubscriptionsAsync(int page = 1, int pageSize = 100);
    Task<(bool Success, SubscriptionDTO? Data, string? Error)> CreateMySubscriptionAsync(string tierName);
    Task<(bool Success, string? Error)> CreateMyPaymentAsync(int subId, decimal amount);

    // Public data (anonymous-safe)
    Task<List<TierDTO>> GetAllTiersAsync();
    Task<PagedResult<ClassDTO>> GetClassesAsync(int page = 1, int pageSize = 100);
    Task<PagedResult<SessionDTO>> GetSessionsAsync(int page = 1, int pageSize = 100);

    // LLM chat
    Task<string?> AskDeltaAsync(string prompt);
}