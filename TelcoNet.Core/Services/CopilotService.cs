using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TelcoNet.Core.Interfaces;
using TelcoNet.Core.Models.DTOs;
using TelcoNet.Data;
using TelcoNet.Data.Entities;

namespace TelcoNet.Core.Services;

public class CopilotService : ICopilotService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletion;
    private readonly AppDbContext _db;

    private const string SystemPrompt = @"You are TelcoNet AI, an intelligent network operations assistant for a telecommunications company operating in Nigeria.

Your capabilities:
- Analyze network performance and health across all regions
- Detect and report on network outages
- Find the best connectivity spots in any area
- Provide KPI summaries and analytics
- Alert on critical network issues

When responding:
- Be concise and professional
- Use data from the tools/plugins available to you
- When reporting metrics, format numbers clearly (e.g., 450ms latency, 3.2% packet loss)
- For outages, always mention severity, affected users, and any ongoing remediation
- Proactively suggest next steps or related queries the user might want to investigate
- If a region is experiencing issues, briefly compare it to healthy regions for context

You serve network engineers, NOC operators, and management — adjust your tone based on the question complexity.";

    public CopilotService(Kernel kernel, AppDbContext db)
    {
        _kernel = kernel;
        _chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
        _db = db;
    }

    public async Task<ChatResponseDto> ChatAsync(int userId, ChatRequestDto request)
    {
        // Find or create session
        ChatSession session;
        if (!string.IsNullOrEmpty(request.SessionId))
        {
            session = await _db.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.SessionId == request.SessionId && s.UserId == userId)
                ?? CreateNewSession(userId);
        }
        else
        {
            session = CreateNewSession(userId);
        }

        // Build chat history from stored messages
        var chatHistory = new ChatHistory(SystemPrompt);

        foreach (var msg in session.Messages.OrderBy(m => m.Timestamp))
        {
            if (msg.Role == "user")
                chatHistory.AddUserMessage(msg.Content);
            else if (msg.Role == "assistant")
                chatHistory.AddAssistantMessage(msg.Content);
        }

        // Add current user message
        chatHistory.AddUserMessage(request.Message);

        // Save user message to DB
        session.Messages.Add(new ChatMessage
        {
            Role = "user",
            Content = request.Message,
            Timestamp = DateTime.UtcNow
        });

        // Auto-generate title from first message
        if (string.IsNullOrEmpty(session.Title))
        {
            session.Title = request.Message.Length > 80
                ? request.Message[..80] + "..."
                : request.Message;
        }

        // Enable automatic function calling (plugins)
        var settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        // Get AI response
        ChatMessageContent result;
        try 
        {
            result = await _chatCompletion.GetChatMessageContentAsync(
                chatHistory,
                executionSettings: settings,
                kernel: _kernel
            );
        }
        catch (Exception ex)
        {
            return new ChatResponseDto
            {
                SessionId = session.SessionId,
                Response = $"⚠️ AI Error: {ex.Message}",
                PluginsUsed = new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }

        var responseText = result.Content ?? "I'm sorry, I couldn't process that request. Please try again.";

        // Track which plugins were used (from metadata)
        var pluginsUsed = new List<string>();
        if (result.Metadata?.ContainsKey("Usage") == true)
        {
            // Extract plugin names from function call metadata if available
        }

        // Save assistant response to DB
        session.Messages.Add(new ChatMessage
        {
            Role = "assistant",
            Content = responseText,
            PluginsUsed = pluginsUsed.Any() ? string.Join(",", pluginsUsed) : null,
            Timestamp = DateTime.UtcNow
        });

        session.LastMessageAt = DateTime.UtcNow;

        // Save to database
        if (session.Id == 0)
            _db.ChatSessions.Add(session);

        await _db.SaveChangesAsync();

        return new ChatResponseDto
        {
            SessionId = session.SessionId,
            Response = responseText,
            PluginsUsed = pluginsUsed,
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<List<ChatSessionDto>> GetSessionsAsync(int userId)
    {
        return await _db.ChatSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LastMessageAt)
            .Select(s => new ChatSessionDto
            {
                SessionId = s.SessionId,
                Title = s.Title,
                CreatedAt = s.CreatedAt,
                LastMessageAt = s.LastMessageAt,
                IsSavedInvestigation = s.IsSavedInvestigation
            })
            .ToListAsync();
    }

    public async Task<ChatSessionDetailDto?> GetSessionAsync(int userId, string sessionId)
    {
        var session = await _db.ChatSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

        if (session == null) return null;

        return new ChatSessionDetailDto
        {
            SessionId = session.SessionId,
            Title = session.Title,
            CreatedAt = session.CreatedAt,
            LastMessageAt = session.LastMessageAt,
            IsSavedInvestigation = session.IsSavedInvestigation,
            Messages = session.Messages
                .OrderBy(m => m.Timestamp)
                .Select(m => new ChatMessageDto
                {
                    Role = m.Role,
                    Content = m.Content,
                    PluginsUsed = m.PluginsUsed,
                    Timestamp = m.Timestamp
                }).ToList()
        };
    }

    public async Task<bool> SaveInvestigationAsync(int userId, string sessionId)
    {
        var session = await _db.ChatSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

        if (session == null) return false;

        session.IsSavedInvestigation = true;
        await _db.SaveChangesAsync();
        return true;
    }

    private static ChatSession CreateNewSession(int userId)
    {
        return new ChatSession
        {
            SessionId = Guid.NewGuid().ToString(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow,
            Messages = new List<ChatMessage>()
        };
    }
}
