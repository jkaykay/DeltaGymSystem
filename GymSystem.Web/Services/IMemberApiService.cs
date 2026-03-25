using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services;

public interface IMemberApiService
{
    // Auth
    Task<LoginResponse?> LoginAsync(string email, string password);

    // Dashboard Stats
    Task<int> GetUpcomingClassesCountAsync(string memberId);
    Task<int> GetTotalBookingsCountAsync(string memberId);
    Task<int> GetClassesAttendedCountAsync(string memberId);

    // Log History
    Task<List<LogItem>> GetLogHistoryAsync(string memberId);

    // Payment History
    Task<List<PaymentItem>> GetPaymentHistoryAsync(string memberId);
}