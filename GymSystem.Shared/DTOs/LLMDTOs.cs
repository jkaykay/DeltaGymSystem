using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GymSystem.Shared.DTOs
{
    public class PromptRequest
    {
        [Required, MinLength(1)]
        public string Prompt { get; set; } = string.Empty;
    }
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