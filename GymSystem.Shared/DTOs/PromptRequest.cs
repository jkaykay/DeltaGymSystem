using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GymSystem.Shared.DTOs
{
    public class PromptRequest
    {
        [Required, MinLength(1)]
        public string Prompt { get; set; } = string.Empty;
    }
}