using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Api.Services;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace GymSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LLMController : ControllerBase
    {
        private readonly IOpenRouterService _openRouterService;

        public LLMController(IOpenRouterService openRouterService)
        {
            _openRouterService = openRouterService;
        }

        /// <summary>POST api/llm/chat — sends a prompt to the LLM and returns the response.</summary>
        [HttpPost("chat")]
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