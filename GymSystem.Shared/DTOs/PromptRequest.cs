using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GymSystem.Shared.DTOs
{
    // Request DTO for sending a text prompt to the AI assistant.
    public class PromptRequest
    {
        // The user prompt text (required, 1–2500 characters).
        [Required, MinLength(1), MaxLength(2500)]
        public string Prompt { get; set; } = string.Empty;
    }
}
