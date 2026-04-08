using System.Net.Http.Headers;
using System.Net.Http.Json;
using GymSystem.Shared.DTOs;
using GymSystem.Web.Areas.Trainer.Models;

namespace GymSystem.Web.Services
{
    /// <summary>
    /// Implements <see cref="ITrainerApiService"/>.
    /// This service handles all API calls for the Trainer area:
    /// profile management, session listing/creation/deletion,
    /// room and class lookups, and viewing session bookings.
    /// Unlike other services, each method takes an explicit JWT token parameter
    /// because the trainer's token is read from the cookie in the controller.
    /// </summary>
    public class TrainerApiService : ITrainerApiService
    {
        // Factory used to create HttpClient instances with the preconfigured API base URL.
        private readonly IHttpClientFactory _httpClientFactory;

        // Constructor — the DI container injects the factory.
        public TrainerApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        /// <summary>
        /// Fetches the currently authenticated trainer's profile from GET api/trainer/me.
        /// The Bearer token identifies which trainer is making the request.
        /// </summary>
        public async Task<UserDTO?> GetTrainerProfileAsync(string token, CancellationToken cancellationToken = default)
        {

            var client = _httpClientFactory.CreateClient("GymApi");

            // Attach the JWT so the API knows which trainer is requesting their profile.
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/trainer/me", cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserDTO>(cancellationToken: cancellationToken);

        }


        /// <summary>
        /// Updates the trainer's own profile (email, phone) via PUT api/trainer/me.
        /// Returns true if the API accepted the update.
        /// </summary>
        public async Task<bool> UpdateTrainerProfileAsync(UpdateTrainerProfileRequest request, string token, CancellationToken cancellationToken = default)
        {

            var client = _httpClientFactory.CreateClient("GymApi");


            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // PUT sends the updated fields as JSON to the API.
            var response = await client.PutAsJsonAsync("api/trainer/me", request, cancellationToken);


            return response.IsSuccessStatusCode;
        }


        /// <summary>
        /// Fetches a paged list of sessions for a specific trainer (by instructorId).
        /// Supports optional search text to filter by class subject.
        /// </summary>
        public async Task<TrainerPagedResult<SessionDTO>> GetSessionsAsync(string instructorId, int page, int pageSize, string token, string? search = null, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Build the query string with paging, instructor filter, and optional search.
            var url = $"api/session?instructorId={Uri.EscapeDataString(instructorId)}&page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";

            var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new TrainerPagedResult<SessionDTO>();

            var result = await response.Content.ReadFromJsonAsync<TrainerPagedResult<SessionDTO>>(cancellationToken: cancellationToken);

            return result ?? new TrainerPagedResult<SessionDTO>();
        }


        /// <summary>Fetches a single session by its ID from GET api/session/{id}.</summary>
        public async Task<SessionDTO?> GetSessionByIdAsync(int sessionId, string token, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"api/session/{sessionId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<SessionDTO>(cancellationToken: cancellationToken);
        }


        /// <summary>
        /// Fetches all bookings (members who signed up) for a specific session.
        /// Used on the session detail page to show the attendee list.
        /// </summary>
        public async Task<List<BookingDTO>> GetSessionBookingsAsync(int sessionId, string token, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"api/session/{sessionId}/bookings", cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new List<BookingDTO>();

            return await response.Content.ReadFromJsonAsync<List<BookingDTO>>(cancellationToken: cancellationToken) ?? new List<BookingDTO>();
        }


        /// <summary>Deletes (cancels) a session by its ID via DELETE api/session/{id}.</summary>
        public async Task<bool> DeleteSessionAsync(int sessionId, string token, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"api/session/{sessionId}", cancellationToken);

            return response.IsSuccessStatusCode;
        }


        /// <summary>
        /// Fetches all rooms that belong to a specific branch.
        /// Used when the trainer creates a session and needs to pick a room.
        /// </summary>
        public async Task<List<RoomDTO>> GetRoomsByBranchAsync(int branchId, string token, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"api/room?branchId={branchId}&page=1&pageSize=999", cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new List<RoomDTO>();

            var result = await response.Content.ReadFromJsonAsync<TrainerPagedResult<RoomDTO>>(cancellationToken: cancellationToken);

            return result?.Items ?? new List<RoomDTO>();
        }


        /// <summary>
        /// Fetches the classes assigned to a specific trainer.
        /// Used when the trainer creates a session and needs to pick which class to teach.
        /// </summary>
        public async Task<List<ClassDTO>> GetTrainerClassesAsync(string trainerId, string token, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"api/class?userId={Uri.EscapeDataString(trainerId)}&page=1&pageSize=999", cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new List<ClassDTO>();

            var result = await response.Content.ReadFromJsonAsync<TrainerPagedResult<ClassDTO>>(cancellationToken: cancellationToken);

            return result?.Items ?? new List<ClassDTO>();
        }


        /// <summary>Creates a new training session via POST api/session.</summary>
        public async Task<bool> CreateSessionAsync(AddSessionRequest request, string token, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsJsonAsync("api/session", request, cancellationToken);

            return response.IsSuccessStatusCode;
        }


        /// <summary>
        /// Fetches a paged list of sessions for a trainer, filtered by optional date range.
        /// Used on the trainer dashboard to show today's and upcoming sessions.
        /// </summary>
        public async Task<TrainerPagedResult<SessionDTO>> GetSessionsByTrainerAsync(string instructorId, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize, string token, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = $"api/session?instructorId={Uri.EscapeDataString(instructorId)}&page={page}&pageSize={pageSize}";

            if (dateFrom.HasValue)
                url += $"&dateFrom={Uri.EscapeDataString(dateFrom.Value.ToString("o"))}";

            if (dateTo.HasValue)
                url += $"&dateTo={Uri.EscapeDataString(dateTo.Value.ToString("o"))}";

            var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new TrainerPagedResult<SessionDTO>();

            var result = await response.Content.ReadFromJsonAsync<TrainerPagedResult<SessionDTO>>(cancellationToken: cancellationToken);

            return result ?? new TrainerPagedResult<SessionDTO>();
        }
    }
}