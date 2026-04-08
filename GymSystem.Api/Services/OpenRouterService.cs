// ============================================================
// OpenRouterService.cs — LLM (AI chatbot) integration service.
// This service implements an "agentic loop": it sends messages
// to an AI model, and if the model requests data (via tool calls),
// this service fetches the data from the database and sends it
// back to the AI for a final response. This powers "DeltaBot".
// ============================================================

using GymSystem.Api.Data;
using GymSystem.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GymSystem.Api.Services
{
    public class OpenRouterService : IOpenRouterService
    {
        private readonly HttpClient _httpClient;          // Sends HTTP requests to the AI API
        private readonly IServiceProvider _serviceProvider; // Resolves scoped services (like DbContext)
        private readonly string _model;                    // Which AI model to use

        // Safety limit: stop after 6 back-and-forth rounds with the AI
        private const int MaxToolRounds = 6;

        private const string SystemPrompt = """
            You are DeltaBot, a helpful gym assistant for Delta Gym System.
            You can look up classes, branches, membership tiers, and sessions.
            Always be concise and friendly. If you cannot find data, say so.
            Use the tools provided when the user asks about gym-specific information.
            Do NOT invent data — only use what the tools return.
            """;

        // ── Tool schemas sent to the LLM ────────────────────────────
        private static readonly List<ToolDefinition> Tools =
        [
            new()
            {
                Function = new FunctionDefinition
                {
                    Name        = "get_classes",
                    Description = "List all gym classes with their subject, trainer name and branch location.",
                    Parameters  = new { type = "object", properties = new { }, required = Array.Empty<string>() }
                }
            },
            new()
            {
                Function = new FunctionDefinition
                {
                    Name        = "get_branches",
                    Description = "List all gym branches with address, city, province and post code.",
                    Parameters  = new { type = "object", properties = new { }, required = Array.Empty<string>() }
                }
            },
            new()
            {
                Function = new FunctionDefinition
                {
                    Name        = "get_tiers",
                    Description = "List all membership tiers with tier name and price.",
                    Parameters  = new { type = "object", properties = new { }, required = Array.Empty<string>() }
                }
            },
            new()
            {
                Function = new FunctionDefinition
                {
                    Name        = "get_schedules",
                    Description = "Get upcoming class sessions. Optionally filter by class subject.",
                    Parameters  = new
                    {
                        type = "object",
                        properties = new
                        {
                            class_subject = new { type = "string", description = "Optional class subject to filter by." }
                        },
                        required = Array.Empty<string>()
                    }
                }
            }
        ];

        // Constructor — configures the HTTP client with the API key and base URL.
        public OpenRouterService(HttpClient httpClient, IConfiguration config, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _serviceProvider = serviceProvider;

            var apiKey = config["OpenRouter:ApiKey"]
                ?? throw new InvalidOperationException("OpenRouter:ApiKey is not configured.");

            _httpClient.BaseAddress = new Uri(config["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            _model = config["OpenRouter:Model"] ?? "nvidia/nemotron-3-super-120b-a12b:free";
        }

        // --- Agentic loop ---
        // Sends the user's prompt to the LLM. If the LLM requests tools
        // (e.g. "get_classes"), we execute them locally and feed the results
        // back. This continues until the LLM gives a final text response.
        // ── Agentic loop ────────────────────────────────────────────
        public async Task<string> GetCompletionAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var messages = new List<ChatMessage>
            {
                new() { Role = "system",  Content = SystemPrompt },
                new() { Role = "user",    Content = prompt }
            };

            for (int round = 0; round < MaxToolRounds; round++)
            {
                var request = new ChatRequest
                {
                    Model = _model,
                    Messages = messages,
                    Tools = Tools
                };

                var response = await _httpClient.PostAsJsonAsync("chat/completions", request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"OpenRouter returned {response.StatusCode}: {errorBody}");
                }

                var result = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken);
                var choice = result?.Choices.FirstOrDefault();

                if (choice is null)
                    return "No response from LLM.";

                // If no tool calls → we have the final answer
                if (choice.Message.ToolCalls is null || choice.Message.ToolCalls.Count == 0)
                    return choice.Message.Content ?? "No response";

                // Append the assistant message (with tool_calls) to history
                messages.Add(choice.Message);

                // Execute each tool call and append results
                foreach (var toolCall in choice.Message.ToolCalls)
                {
                    var toolResult = await ExecuteToolAsync(toolCall.Function.Name,
                                                            toolCall.Function.Arguments,
                                                            cancellationToken);
                    messages.Add(new ChatMessage
                    {
                        Role = "tool",
                        Content = toolResult,
                        ToolCallId = toolCall.Id
                    });
                }
                // Loop back — the LLM now sees the tool results
            }

            return "I wasn't able to finish processing. Please try a simpler question.";
        }

        // --- Tool dispatcher ---
        // When the LLM calls a tool by name (e.g. "get_classes"), this method
        // maps the name to a handler that queries the database and returns JSON.
        // ── Tool dispatcher ─────────────────────────────────────────
        private async Task<string> ExecuteToolAsync(string toolName, string? argsJson, CancellationToken ct)
        {
            // Create a scope so we can resolve scoped services (DbContext)
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GymDbContext>();

            return toolName switch
            {
                "get_classes" => await HandleGetClasses(db, ct),
                "get_branches" => await HandleGetBranches(db, ct),
                "get_tiers" => await HandleGetTiers(db, ct),
                "get_schedules" => await HandleGetSchedules(db, argsJson, ct),
                _ => JsonSerializer.Serialize(new { error = $"Unknown tool: {toolName}" })
            };
        }

        // Fetches all gym classes with trainer name and branch info.
        private static async Task<string> HandleGetClasses(GymDbContext db, CancellationToken ct)
        {
            var classes = await db.Classes
                .Include(c => c.User)
                    .ThenInclude(u => u.Branch)
                .Select(c => new
                {
                    c.Subject,
                    Trainer = c.User.FirstName + " " + c.User.LastName,
                    Branch = c.User.Branch != null
                        ? c.User.Branch.City + ", " + c.User.Branch.Address
                        : "Unassigned"
                })
                .ToListAsync(ct);

            return JsonSerializer.Serialize(classes);
        }

        // Fetches all gym branch locations.
        private static async Task<string> HandleGetBranches(GymDbContext db, CancellationToken ct)
        {
            var branches = await db.Branches
                .Select(b => new { b.Address, b.City, b.Province, b.PostCode })
                .ToListAsync(ct);

            return JsonSerializer.Serialize(branches);
        }

        // Fetches all membership tiers with prices.
        private static async Task<string> HandleGetTiers(GymDbContext db, CancellationToken ct)
        {
            var tiers = await db.Tiers
                .Select(t => new { t.TierName, t.Price })
                .ToListAsync(ct);

            return JsonSerializer.Serialize(tiers);
        }

        // Fetches upcoming sessions, optionally filtered by class subject.
        private static async Task<string> HandleGetSchedules(GymDbContext db, string? argsJson, CancellationToken ct)
        {
            string? classSubject = null;
            if (!string.IsNullOrWhiteSpace(argsJson))
            {
                using var doc = JsonDocument.Parse(argsJson);
                if (doc.RootElement.TryGetProperty("class_subject", out var cs))
                    classSubject = cs.GetString();
            }

            var query = db.Sessions
                .Include(s => s.Class)
                .Include(s => s.Room)
                .Where(s => s.Start >= DateTime.UtcNow)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(classSubject))
                query = query.Where(s => s.Class.Subject.Contains(classSubject));

            var sessions = await query
                .OrderBy(s => s.Start)
                .Take(10)
                .Select(s => new
                {
                    Subject = s.Class.Subject,
                    RoomNumber = s.Room.RoomNumber,
                    s.Start,
                    s.End,
                    s.MaxCapacity
                })
                .ToListAsync(ct);

            return JsonSerializer.Serialize(sessions);
        }

        // ── DTOs (OpenAI-compatible schema) ─────────────────────────
        public class ChatRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("messages")]
            public List<ChatMessage> Messages { get; set; } = [];

            [JsonPropertyName("tools")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public List<ToolDefinition>? Tools { get; set; }
        }

        public class ChatMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = "user";

            [JsonPropertyName("content")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Content { get; set; }

            [JsonPropertyName("tool_calls")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public List<ToolCall>? ToolCalls { get; set; }

            [JsonPropertyName("tool_call_id")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? ToolCallId { get; set; }
        }

        public class ChatResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; } = [];
        }

        public class Choice
        {
            [JsonPropertyName("message")]
            public ChatMessage Message { get; set; } = new();
        }

        public class ToolDefinition
        {
            [JsonPropertyName("type")]
            public string Type { get; set; } = "function";

            [JsonPropertyName("function")]
            public FunctionDefinition Function { get; set; } = new();
        }

        public class FunctionDefinition
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;

            [JsonPropertyName("parameters")]
            public object Parameters { get; set; } = new { };
        }

        public class ToolCall
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("type")]
            public string Type { get; set; } = "function";

            [JsonPropertyName("function")]
            public FunctionCall Function { get; set; } = new();
        }

        public class FunctionCall
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("arguments")]
            public string? Arguments { get; set; }
        }
    }
}