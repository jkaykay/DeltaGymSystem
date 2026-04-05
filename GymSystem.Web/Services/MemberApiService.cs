using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services;

public class MemberApiService : IMemberApiService
{
    private readonly HttpClient _http;

    public MemberApiService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("GymApi");
    }

    // --- Auth ---

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

    // --- Profile ---

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
            LastName: lastName,
            Telephone: model.Telephone,
            EmergencyContact: model.EmergencyContact,
            Weight: model.Weight
        );

        var response = await _http.PutAsJsonAsync("api/member/update", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        return (true, null);
    }

    // --- QR ---

    public async Task<QRCodeResponse?> GetMyQRAsync(string id)
    {
        var code = await _http.GetAsync($"api/QRCode/generate/{id}");
        if (!code.IsSuccessStatusCode) return null;

        return await code.Content.ReadFromJsonAsync<QRCodeResponse>();
    }

    // --- Dashboard data ---

    public async Task<PagedResult<BookingDTO>> GetMyBookingsAsync(int page = 1, int pageSize = 100)
    {
        var result = await _http.GetFromJsonAsync<PagedResult<BookingDTO>>($"api/booking/my?page={page}&pageSize={pageSize}");
        return result ?? new PagedResult<BookingDTO>();
    }

    public async Task<List<AttendanceDTO>> GetMyAttendancesAsync(string memberId)
    {
        var result = await _http.GetFromJsonAsync<List<AttendanceDTO>>($"api/attendance/member/{memberId}");
        return result ?? [];
    }

    public async Task<PagedResult<PaymentDTO>> GetMyPaymentsAsync(int page = 1, int pageSize = 100)
    {
        var result = await _http.GetFromJsonAsync<PagedResult<PaymentDTO>>($"api/payment/my?page={page}&pageSize={pageSize}");
        return result ?? new PagedResult<PaymentDTO>();
    }

    // --- Booking ---

    public async Task<PagedResult<SessionDTO>> GetUpcomingSessionsAsync(int page = 1, int pageSize = 100)
    {
        var from = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
        var result = await _http.GetFromJsonAsync<PagedResult<SessionDTO>>($"api/session?page={page}&pageSize={pageSize}&dateFrom={from}&sortBy=start&sortDir=asc");
        return result ?? new PagedResult<SessionDTO>();
    }

    public async Task<(bool Success, string? Error)> CreateMyBookingAsync(int sessionId)
    {
        var response = await _http.PostAsJsonAsync("api/booking/my", new AddMyBookingRequest(sessionId));

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CancelMyBookingAsync(int bookingId)
    {
        var response = await _http.DeleteAsync($"api/booking/my/{bookingId}");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        return (true, null);
    }

    // --- Subscriptions ---

    public async Task<PagedResult<SubscriptionDTO>> GetMySubscriptionsAsync(int page = 1, int pageSize = 100)
    {
        var result = await _http.GetFromJsonAsync<PagedResult<SubscriptionDTO>>($"api/subscription/my?page={page}&pageSize={pageSize}");
        return result ?? new PagedResult<SubscriptionDTO>();
    }

    public async Task<(bool Success, SubscriptionDTO? Data, string? Error)> CreateMySubscriptionAsync(string tierName)
    {
        var response = await _http.PostAsJsonAsync("api/subscription/my", new AddMySubscriptionRequest(tierName));

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return (false, null, error);
        }

        var data = await response.Content.ReadFromJsonAsync<SubscriptionDTO>();
        return (true, data, null);
    }

    public async Task<(bool Success, string? Error)> CreateMyPaymentAsync(int subId, decimal amount)
    {
        var response = await _http.PostAsJsonAsync("api/payment/my", new AddMyPaymentRequest(amount, subId));

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        return (true, null);
    }

    // --- Public data (anonymous) ---

    public async Task<List<TierDTO>> GetAllTiersAsync()
    {
        var result = await _http.GetFromJsonAsync<PagedResult<TierDTO>>("api/tier?page=1&pageSize=50");
        return result?.Items ?? [];
    }

    public async Task<PagedResult<ClassDTO>> GetClassesAsync(int page = 1, int pageSize = 100)
    {
        var result = await _http.GetFromJsonAsync<PagedResult<ClassDTO>>($"api/class?page={page}&pageSize={pageSize}");
        return result ?? new PagedResult<ClassDTO>();
    }

    public async Task<PagedResult<SessionDTO>> GetSessionsAsync(int page = 1, int pageSize = 100)
    {
        var result = await _http.GetFromJsonAsync<PagedResult<SessionDTO>>($"api/session?page={page}&pageSize={pageSize}");
        return result ?? new PagedResult<SessionDTO>();
    }

    // --- LLM chat ---

    public async Task<string?> AskDeltaAsync(string prompt)
    {
        var response = await _http.PostAsJsonAsync("api/llm/chat", new PromptRequest { Prompt = prompt });

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<LlmChatResponse>();
        return result?.Response;
    }

    private sealed class LlmChatResponse
    {
        public string Response { get; set; } = string.Empty;
    }
}