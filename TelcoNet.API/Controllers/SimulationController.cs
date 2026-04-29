using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelcoNet.Data;
using TelcoNet.Data.Entities;

namespace TelcoNet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SimulationController : ControllerBase
{
    private readonly AppDbContext _db;

    public SimulationController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("nodes/status")]
    public async Task<IActionResult> SetNodeStatus([FromBody] StatusRequest request)
    {
        var node = await _db.NetworkNodes.FirstOrDefaultAsync(n => n.NodeId == request.NodeId);
        if (node == null) return NotFound(new { error = "Node not found" });

        node.Status = request.Status;
        node.LastUpdated = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Node {request.NodeId} status updated to {request.Status}" });
    }

    [HttpPost("trigger-outage")]
    public async Task<IActionResult> TriggerOutage([FromBody] OutageRequest request)
    {
        var node = await _db.NetworkNodes.FirstOrDefaultAsync(n => n.NodeId == request.NodeId);
        if (node == null) return NotFound(new { error = "Node not found" });

        node.Status = NodeStatus.Down;
        node.LastUpdated = DateTime.UtcNow;

        var outage = new Outage
        {
            AffectedNodeId = request.NodeId,
            Region = node.Region,
            Reason = request.Reason,
            Severity = request.Severity,
            StartedAt = DateTime.UtcNow,
            EstimatedUsersAffected = new Random().Next(1000, 50000)
        };

        var alert = new Alert
        {
            Title = $"CRITICAL: Outage detected at {node.Name}",
            Description = $"Root Cause: {request.Reason}. Impacting {outage.EstimatedUsersAffected} users.",
            Severity = "Critical",
            Region = node.Region,
            CreatedAt = DateTime.UtcNow
        };

        _db.Outages.Add(outage);
        _db.Alerts.Add(alert);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Outage triggered successfully", outageId = outage.Id });
    }

    [HttpPost("trigger-alert")]
    public async Task<IActionResult> TriggerAlert([FromBody] AlertRequest request)
    {
        var alert = new Alert
        {
            Title = request.Title,
            Description = request.Description,
            Severity = request.Severity,
            Region = request.Region,
            CreatedAt = DateTime.UtcNow
        };

        _db.Alerts.Add(alert);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Alert triggered successfully" });
    }

    [HttpPost("resolve-outage")]
    public async Task<IActionResult> ResolveOutage()
    {
        var outage = await _db.Outages
            .Where(o => o.ResolvedAt == null)
            .OrderByDescending(o => o.StartedAt)
            .FirstOrDefaultAsync();

        if (outage == null) return NotFound(new { error = "No active outages found." });

        outage.ResolvedAt = DateTime.UtcNow;

        var node = await _db.NetworkNodes.FirstOrDefaultAsync(n => n.NodeId == outage.AffectedNodeId);
        if (node != null)
        {
            node.Status = NodeStatus.Healthy;
            node.LastUpdated = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = $"Outage at {node?.Name ?? "Unknown"} resolved successfully" });
    }
}

public record StatusRequest(string NodeId, NodeStatus Status);
public record OutageRequest(string NodeId, string Reason, string Severity = "Critical");
public record AlertRequest(string Title, string Description, string Severity, string Region);
