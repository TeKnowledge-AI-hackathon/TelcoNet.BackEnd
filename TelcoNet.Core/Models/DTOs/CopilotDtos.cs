namespace TelcoNet.Core.Models.DTOs;

// ── Copilot / Chat DTOs ──

public class ChatRequestDto
{
    public string? SessionId { get; set; } // null = new session
    public string Message { get; set; } = string.Empty;
}

public class ChatResponseDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public List<string> PluginsUsed { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ChatSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
    public bool IsSavedInvestigation { get; set; }
}

public class ChatSessionDetailDto : ChatSessionDto
{
    public List<ChatMessageDto> Messages { get; set; } = new();
}

public class ChatMessageDto
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? PluginsUsed { get; set; }
    public DateTime Timestamp { get; set; }
}
