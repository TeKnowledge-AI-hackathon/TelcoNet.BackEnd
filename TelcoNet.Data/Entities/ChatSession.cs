using System.ComponentModel.DataAnnotations;

namespace TelcoNet.Data.Entities;

public class ChatSession
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string SessionId { get; set; } = Guid.NewGuid().ToString();

    public int UserId { get; set; }
    public User? User { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; } // Auto-generated from first message

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    public bool IsSavedInvestigation { get; set; } = false;

    public List<ChatMessage> Messages { get; set; } = new();
}

public class ChatMessage
{
    [Key]
    public int Id { get; set; }

    public int ChatSessionId { get; set; }
    public ChatSession? ChatSession { get; set; }

    [Required, MaxLength(10)]
    public string Role { get; set; } = "user"; // "user", "assistant", "system"

    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>Which SK plugins were invoked to generate this response</summary>
    [MaxLength(500)]
    public string? PluginsUsed { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
