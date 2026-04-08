// ============================================================
// IOpenRouterService.cs — Interface for the LLM (AI chatbot) service.
// Defines a single method that sends a user's prompt to an
// external AI model (via OpenRouter) and returns the response.
// ============================================================

namespace GymSystem.Api.Services
{
    public interface IOpenRouterService
    {
        // Send a prompt to the LLM and return the text response.
        Task<string> GetCompletionAsync(string prompt, CancellationToken cancellationToken = default);
    }
}