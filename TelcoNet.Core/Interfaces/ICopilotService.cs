using TelcoNet.Core.Models.DTOs;

namespace TelcoNet.Core.Interfaces;

public interface ICopilotService
{
    Task<ChatResponseDto> ChatAsync(int userId, ChatRequestDto request);
    Task<List<ChatSessionDto>> GetSessionsAsync(int userId);
    Task<ChatSessionDetailDto?> GetSessionAsync(int userId, string sessionId);
    Task<bool> SaveInvestigationAsync(int userId, string sessionId);
}
