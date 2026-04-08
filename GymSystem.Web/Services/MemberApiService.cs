using System.Text.Json;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services;

/// <summary>
/// Implements <see cref="IMemberApiService"/>.
/// This service handles all member-facing API calls: login, registration,
/// profile management, QR codes, bookings, subscriptions, payments,
/// and the AI chat feature (AskDelta).
/// It uses a preconfigured HttpClient ("GymApi") that already includes
/// the base URL and the TokenDelegatingHandler for automatic JWT attachment.
/// </summary>
public class MemberApiService : IMemberApiService
{
    // The HttpClient used to call the backend REST API.
    private readonly HttpClient _http;

    // Constructor — the DI container injects IHttpClientFactory, from which
    // we create the named "GymApi" client (base URL + token handler preconfigured).
    public MemberApiService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("GymApi");
    }

    // =====================================================================
    // AUTH — login and registration
    // =====================================================================

    /// <summary>
    /// Sends login credentials to POST api/auth/login.
    /// Returns a tuple: (Success, LoginResponse data, Error message).
    /// </summary>
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

    /// <summary>
    /// Registers a new member by POSTing to api/auth/register.
    /// If the API returns Identity validation errors (e.g. "Password too short"),
    /// they are extracted from the JSON array and combined into a single message.
    /// </summary>
    public async Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", request);

        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync();

            // Try to parse as a JSON array of identity errors and extract descriptions
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var descriptions = doc.RootElement
                        .EnumerateArray()
                        .Where(e => e.TryGetProperty("description", out _))
                        .Select(e => e.GetProperty("description").GetString())
                        .Where(d => !string.IsNullOrEmpty(d));

                    var joined = string.Join(" ", descriptions);
                    if (!string.IsNullOrEmpty(joined))
                        return (false, joined);
                }
            }
            catch (JsonException) { }

            return (false, raw);
        }

        return (true, null);
    }

    // =====================================================================
    // PROFILE — view and update the logged-in member's profile
    // =====================================================================

    /// <summary>
    /// Fetches the current member's profile from GET api/auth/me.
    /// The JWT token (sent automatically) tells the API which user we are.
    /// </summary>
    public async Task<UserDTO?> GetMyProfileAsync()
    {
        var response = await _http.GetAsync("api/auth/me");

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<UserDTO>();
    }

    /// <summary>
    /// Updates the member's profile (email, name, phone) via PUT api/member/{id}.
    /// </summary>
    public async Task<(bool Success, string? Error)> UpdateProfileAsync(string memberId, UpdateMemberRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/member/{memberId}", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        return (true, null);
    }

    // =====================================================================
    // QR — generate a QR code for gym check-in
    // =====================================================================

    /// <summary>
    /// Requests a time-limited QR code image (base64) for the given member.
    /// The QR code can be scanned at the gym entrance for check-in.
    /// </summary>
    public async Task<QRCodeResponse?> GetMyQRAsync(string id)
    {
        var code = await _http.GetAsync($"api/QRCode/generate/{id}");
        if (!code.IsSuccessStatusCode) return null;

        return await code.Content.ReadFromJsonAsync<QRCodeResponse>();
    }

    // =====================================================================
    // DASHBOARD DATA — bookings, attendances, payments for the logged-in member
    // =====================================================================

    /// <summary>Fetches the member's bookings from GET api/booking/my.</summary>
    public async Task<PagedResult<BookingDTO>> GetMyBookingsAsync(int page = 1, int pageSize = 100, string? search = null)
    {
        var url = $"api/booking/my?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        var result = await _http.GetFromJsonAsync<PagedResult<BookingDTO>>(url);
        return result ?? new PagedResult<BookingDTO>();
    }

    /// <summary>Fetches attendance (check-in/out) records for the member.</summary>
    public async Task<List<AttendanceDTO>> GetMyAttendancesAsync(string memberId)
    {
        var result = await _http.GetFromJsonAsync<List<AttendanceDTO>>($"api/attendance/member/{memberId}");
        return result ?? [];
    }

    /// <summary>Fetches the member's payment history.</summary>
    public async Task<PagedResult<PaymentDTO>> GetMyPaymentsAsync(int page = 1, int pageSize = 100)
    {
        var result = await _http.GetFromJsonAsync<PagedResult<PaymentDTO>>($"api/payment/my?page={page}&pageSize={pageSize}");
        return result ?? new PagedResult<PaymentDTO>();
    }

    // =====================================================================
    // BOOKING — browse upcoming sessions and book/cancel
    // =====================================================================

    /// <summary>Gets upcoming sessions (starting from now, sorted ascending) for the member to book.</summary>
    public async Task<PagedResult<SessionDTO>> GetUpcomingSessionsAsync(int page = 1, int pageSize = 100, string? search = null)
    {
        var from = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
        var url = $"api/session?page={page}&pageSize={pageSize}&dateFrom={from}&sortBy=start&sortDir=asc";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        var result = await _http.GetFromJsonAsync<PagedResult<SessionDTO>>(url);
        return result ?? new PagedResult<SessionDTO>();
    }

    /// <summary>Books the member into a session by POSTing to api/booking/my.</summary>
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

    /// <summary>Cancels an existing booking for the member.</summary>
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

    // =====================================================================
    // SUBSCRIPTIONS — view plans, subscribe, and pay
    // =====================================================================

    /// <summary>Gets the member's existing subscriptions.</summary>
    public async Task<PagedResult<SubscriptionDTO>> GetMySubscriptionsAsync(int page = 1, int pageSize = 100)
    {
        var result = await _http.GetFromJsonAsync<PagedResult<SubscriptionDTO>>($"api/subscription/my?page={page}&pageSize={pageSize}");
        return result ?? new PagedResult<SubscriptionDTO>();
    }

    /// <summary>Creates a new subscription for the member under the specified tier.</summary>
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

    /// <summary>Records a payment against a subscription to activate it.</summary>
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

    // =====================================================================
    // PUBLIC DATA — anonymous-safe endpoints (no login required)
    // =====================================================================

    /// <summary>Fetches all membership tiers (pricing plans) from the API.</summary>
    public async Task<List<TierDTO>> GetAllTiersAsync()
    {
        var result = await _http.GetFromJsonAsync<PagedResult<TierDTO>>("api/tier?page=1&pageSize=50");
        return result?.Items ?? [];
    }

    /// <summary>Fetches a random selection of trainers to feature on the homepage.</summary>
    public async Task<List<UserDTO>> GetRandomTrainersAsync(int count = 3)
    {
        var result = await _http.GetFromJsonAsync<List<UserDTO>>($"api/trainer/random?count={count}");
        return result ?? [];
    }

    /// <summary>Fetches a paged list of gym classes (public, no auth needed).</summary>
    public async Task<PagedResult<ClassDTO>> GetClassesAsync(int page = 1, int pageSize = 100)
    {
        var result = await _http.GetFromJsonAsync<PagedResult<ClassDTO>>($"api/class?page={page}&pageSize={pageSize}");
        return result ?? new PagedResult<ClassDTO>();
    }

    /// <summary>Fetches a paged list of sessions (public, no auth needed).</summary>
    public async Task<PagedResult<SessionDTO>> GetSessionsAsync(int page = 1, int pageSize = 100)
    {
        var result = await _http.GetFromJsonAsync<PagedResult<SessionDTO>>($"api/session?page={page}&pageSize={pageSize}");
        return result ?? new PagedResult<SessionDTO>();
    }

    // =====================================================================
    // LLM CHAT — AI-powered Q&A ("Ask Delta")
    // =====================================================================

    /// <summary>
    /// Sends a free-text prompt to the backend LLM endpoint and returns the AI's answer.
    /// Returns null if the API call fails.
    /// </summary>
    public async Task<string?> AskDeltaAsync(string prompt)
    {
        var response = await _http.PostAsJsonAsync("api/llm/chat", new PromptRequest { Prompt = prompt });

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<LlmChatResponse>();
        return result?.Response;
    }

    /// <summary>
    /// Small helper class that mirrors the shape of the LLM API response JSON.
    /// </summary>
    private sealed class LlmChatResponse
    {
        public string Response { get; set; } = string.Empty;
    }
}