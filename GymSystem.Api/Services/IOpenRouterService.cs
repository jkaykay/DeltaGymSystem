namespace GymSystem.Api.Services
{
    public interface IOpenRouterService
    {
        Task<string> GetCompletionAsync(string prompt, CancellationToken cancellationToken);
    }
}