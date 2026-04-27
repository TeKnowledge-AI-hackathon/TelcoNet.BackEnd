using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelcoNet.Core.Interfaces;

namespace TelcoNet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NetworkController : ControllerBase
{
    private readonly INetworkService _networkService;

    public NetworkController(INetworkService networkService)
    {
        _networkService = networkService;
    }

    /// <summary>Get overall network health summary (System Healthy indicator).</summary>
    [HttpGet("health")]
    public async Task<IActionResult> GetHealthSummary()
    {
        var summary = await _networkService.GetHealthSummaryAsync();
        return Ok(summary);
    }

    /// <summary>Get network nodes with tower statuses (for Map view).</summary>
    [HttpGet("nodes")]
    public async Task<IActionResult> GetNodes([FromQuery] string? region = null)
    {
        var nodes = await _networkService.GetNodesAsync(region);
        return Ok(new { region = region ?? "All", nodes });
    }

    /// <summary>Get active outages.</summary>
    [HttpGet("outages")]
    public async Task<IActionResult> GetOutages([FromQuery] string? region = null)
    {
        var outages = await _networkService.GetOutagesAsync(region);
        return Ok(outages);
    }

    /// <summary>Get incident timeline (for Timeline view).</summary>
    [HttpGet("timeline")]
    public async Task<IActionResult> GetTimeline([FromQuery] string? region = null)
    {
        var timeline = await _networkService.GetIncidentTimelineAsync(region);
        return Ok(new { incidents = timeline });
    }

    /// <summary>Get alerts, optionally filtered by severity.</summary>
    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts([FromQuery] string? severity = null, [FromQuery] string? region = null)
    {
        var alerts = await _networkService.GetAlertsAsync(severity, region);
        return Ok(alerts);
    }

    /// <summary>Acknowledge an alert.</summary>
    [HttpPut("alerts/{id}/acknowledge")]
    public async Task<IActionResult> AcknowledgeAlert(int id)
    {
        var success = await _networkService.AcknowledgeAlertAsync(id);
        if (!success) return NotFound(new { error = "Alert not found." });
        return Ok(new { message = "Alert acknowledged." });
    }
}
