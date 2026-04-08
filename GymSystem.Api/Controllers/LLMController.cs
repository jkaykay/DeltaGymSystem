// ============================================================
// LLMController.cs — AI chatbot endpoint ("DeltaBot").
// Forwards user prompts to the OpenRouter LLM service and returns
// the AI's response. Rate-limited to 5 requests per minute.
// ============================================================

using GymSystem.Api.Services;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GymSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LLMController : ControllerBase
    {
        private readonly IOpenRouterService _openRouterService; // Service that talks to the AI

        public LLMController(IOpenRouterService openRouterService)
        {
            _openRouterService = openRouterService;
        }

        // POST api/llm/chat — Send a prompt to the AI chatbot and get a response.
        // POST api/llm/chat — sends a prompt to the LLM and returns the response.
        [HttpPost("chat")]
        [EnableRateLimiting("llm")]
        public async Task<IActionResult> Chat([FromBody] PromptRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var reply = await _openRouterService.GetCompletionAsync(request.Prompt, cancellationToken);
                return Ok(new { response = reply });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { error = "LLM service unavailable.", detail = ex.Message });
            }
        }
    }
}
