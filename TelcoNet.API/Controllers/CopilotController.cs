using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelcoNet.Core.Interfaces;
using TelcoNet.Core.Models.DTOs;

namespace TelcoNet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CopilotController : ControllerBase
{
    private readonly ICopilotService _copilotService;

    public CopilotController(ICopilotService copilotService)
    {
        _copilotService = copilotService;
    }

    /// <summary>Send a message to the AI Copilot. Returns AI response.</summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
    {
        var userId = GetUserId();
        var result = await _copilotService.ChatAsync(userId, request);
        return Ok(result);
    }

    /// <summary>Get all chat sessions for the current user (Recent Queries sidebar).</summary>
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        var userId = GetUserId();
        var sessions = await _copilotService.GetSessionsAsync(userId);
        return Ok(sessions);
    }

    /// <summary>Get full chat history for a specific session.</summary>
    [HttpGet("sessions/{sessionId}")]
    public async Task<IActionResult> GetSession(string sessionId)
    {
        var userId = GetUserId();
        var session = await _copilotService.GetSessionAsync(userId, sessionId);
        if (session == null) return NotFound(new { error = "Session not found." });
        return Ok(session);
    }

    /// <summary>Save a session as an investigation (Saved Investigations sidebar).</summary>
    [HttpPut("sessions/{sessionId}/save")]
    public async Task<IActionResult> SaveInvestigation(string sessionId)
    {
        var userId = GetUserId();
        var success = await _copilotService.SaveInvestigationAsync(userId, sessionId);
        if (!success) return NotFound(new { error = "Session not found." });
        return Ok(new { message = "Session saved as investigation." });
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 0;
    }
}
