using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Shared.DTOs;
using System.Runtime.InteropServices;

namespace GymSystem.Web.Services;

public class MemberApiService : IMemberApiService
{
    private readonly HttpClient _http;

    public MemberApiService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("GymApi");
    }

    public async Task<(bool Success, LoginResponse? Data, string? Error)> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return (false, null, error);
        }

        var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return (true, data, null);
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        return (true, null);
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

    public async Task<QRCodeResponse?> GetMyQRAsync(string id)
    {
        var code = await _http.GetAsync($"api/QRCode/generate/{id}");
        if (!code.IsSuccessStatusCode) return null;

        return await code.Content.ReadFromJsonAsync<QRCodeResponse>();
    }

    public async Task<ProfileDto?> GetMyProfileAsync()
{
    var response = await _http.GetAsync("api/member/profile");

    if (!response.IsSuccessStatusCode)
        return null;

    return await response.Content.ReadFromJsonAsync<ProfileDto>();
}

public async Task<(bool Success, string? Error)> UpdateProfileAsync(ProfileViewModel model)
{
    var names = model.FullName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

    var firstName = names.Length > 0 ? names[0] : null;
    var lastName = names.Length > 1 ? names[1] : null;

    var request = new UpdateMemberRequest(
        Email: model.Email,
        FirstName: firstName,
        LastName: lastName
    );

    var response = await _http.PutAsJsonAsync("api/member/update", request);

    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadAsStringAsync();
        return (false, error);
    }

    return (true, null);
}
}