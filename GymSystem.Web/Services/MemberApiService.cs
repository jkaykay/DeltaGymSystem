using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services;

public class MemberApiService : IMemberApiService
{
    private readonly HttpClient _http;

    public MemberApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new { email, password });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    public async Task<int> GetUpcomingClassesCountAsync(string memberId)
    {
        var response = await _http.GetFromJsonAsync<CountResponse>($"api/members/{memberId}/upcoming-classes/count");
        return response?.Count ?? 0;
    }

    public async Task<int> GetTotalBookingsCountAsync(string memberId)
    {
        var response = await _http.GetFromJsonAsync<CountResponse>($"api/members/{memberId}/bookings/count");
        return response?.Count ?? 0;
    }

    public async Task<int> GetClassesAttendedCountAsync(string memberId)
    {
        var response = await _http.GetFromJsonAsync<CountResponse>($"api/members/{memberId}/attended/count");
        return response?.Count ?? 0;
    }

    public async Task<List<LogItem>> GetLogHistoryAsync(string memberId)
    {
        var response = await _http.GetFromJsonAsync<List<LogItem>>($"api/members/{memberId}/logs");
        return response ?? new List<LogItem>();
    }

    public async Task<List<PaymentItem>> GetPaymentHistoryAsync(string memberId)
    {
        var response = await _http.GetFromJsonAsync<List<PaymentItem>>($"api/members/{memberId}/payments");
        return response ?? new List<PaymentItem>();
    }
}