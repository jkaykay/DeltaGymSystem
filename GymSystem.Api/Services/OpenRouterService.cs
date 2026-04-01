using GymSystem.Shared.DTOs;
using System.Text.Json.Serialization;

namespace GymSystem.Api.Services
{
    public class OpenRouterService : IOpenRouterService
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

        public async Task<string> GetCompletionAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var requestBody = new ChatRequest
            {
                Messages = new List<ChatMessage>
                {
                    new() { Role = "user", Content = prompt }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("chat/completions", requestBody, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"OpenRouter returned {response.StatusCode}: {errorBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken);
            return result?.Choices.FirstOrDefault()?.Message.Content ?? "No response";
        }

        //OpenRouter Specific DTOs (Not needed by frontend)
        public class ChatRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = "nvidia/nemotron-3-super-120b-a12b:free";

            [JsonPropertyName("messages")]
            public List<ChatMessage> Messages { get; set; } = new();
        }

        public record ChatMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; init; } = "user";

            [JsonPropertyName("content")]
            public string Content { get; init; } = string.Empty;
        }

        public class ChatResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; } = new();
        }

        public record Choice
        {
            [JsonPropertyName("message")]
            public ChatMessage Message { get; init; } = new();
        }
    }
}