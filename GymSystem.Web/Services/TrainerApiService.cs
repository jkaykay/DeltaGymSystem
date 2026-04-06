using System.Net.Http.Headers;
using System.Net.Http.Json;
using GymSystem.Shared.DTOs;
using GymSystem.Web.Areas.Trainer.Models;

namespace GymSystem.Web.Services
{
    public class TrainerApiService : ITrainerApiService
    {
        //create httpclient
        private readonly IHttpClientFactory _httpClientFactory;

        //constructor 
        public TrainerApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        public async Task<UserDTO?> GetTrainerProfileAsync(string token, CancellationToken cancellationToken = default)
        {

            var client = _httpClientFactory.CreateClient("GymApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/trainer/me", cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserDTO>(cancellationToken: cancellationToken);

        }


        //send request to the api and check if updateprofile worked or not
        public async Task<bool> UpdateTrainerProfileAsync(UpdateTrainerProfileRequest request, string token, CancellationToken cancellationToken = default)
        {

            var client = _httpClientFactory.CreateClient("GymApi");


            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            //send HTTP PUT request
            var response = await client.PutAsJsonAsync("api/trainer/me", request, cancellationToken);


            return response.IsSuccessStatusCode;
        }


        //dashboard service
        public async Task<TrainerPagedResult<SessionDTO>> GetSessionsAsync(string instructorId, int page, int pageSize, string token, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"api/session?instructorId={Uri.EscapeDataString(instructorId)}&page={page}&pageSize={pageSize}", cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new TrainerPagedResult<SessionDTO>();

            var result = await response.Content.ReadFromJsonAsync<TrainerPagedResult<SessionDTO>>(cancellationToken: cancellationToken);

            return result ?? new TrainerPagedResult<SessionDTO>();
        }


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


        public async Task<bool> DeleteSessionAsync(int sessionId, string token, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"api/session/{sessionId}", cancellationToken);

            return response.IsSuccessStatusCode;
        }


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


        public async Task<bool> CreateSessionAsync(AddSessionRequest request, string token, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsJsonAsync("api/session", request, cancellationToken);

            return response.IsSuccessStatusCode;
        }


        public async Task<TrainerPagedResult<SessionDTO>> GetSessionsByTrainerAsync(string instructorId, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize, string token, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = $"api/session?instructorId={Uri.EscapeDataString(instructorId)}&page={page}&pageSize={pageSize}";

            if (dateFrom.HasValue)
                url += $"&dateFrom={dateFrom.Value:o}";

            if (dateTo.HasValue)
                url += $"&dateTo={dateTo.Value:o}";

            var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new TrainerPagedResult<SessionDTO>();

            var result = await response.Content.ReadFromJsonAsync<TrainerPagedResult<SessionDTO>>(cancellationToken: cancellationToken);

            return result ?? new TrainerPagedResult<SessionDTO>();
        }
    }
}