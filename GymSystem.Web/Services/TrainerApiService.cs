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
        public async Task<List<SessionDTO>> GetSessionsAsync(string token, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/session?page=1&pageSize=1000", cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new List<SessionDTO>();

            var result = await response.Content.ReadFromJsonAsync<TrainerPagedResult<SessionDTO>>(cancellationToken: cancellationToken);

            return result?.Items ?? new List<SessionDTO>();
        }
    }
}