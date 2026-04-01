using GymSystem.Shared.DTOs;

namespace GymSystem.Api.Services
{
    public class OpenRouterService
    {
        private readonly HttpClient _httpClient;

        public OpenRouterService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;

            var apiKey = config["OpenRouter:ApiKey"]
                ?? throw new InvalidOperationException("OpenRouter:ApiKey is not configured.");

            _httpClient.BaseAddress = new Uri(config["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1/");

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public async Task<string> GetCompletionAsync(string prompt)
        {
            var requestBody = new ChatRequest
            {
                Messages = new List<ChatMessage>
                {
                    new() { Role = "user", Content = prompt }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("chat/completions", requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ChatResponse>();
            return result?.Choices.FirstOrDefault()?.Message.Content ?? "No response";
        }
    }
}