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

    [HttpPost("nodes/{nodeId}/status")]
    public async Task<IActionResult> SetNodeStatus(string nodeId, [FromQuery] NodeStatus status)
    {
        var node = await _db.NetworkNodes.FirstOrDefaultAsync(n => n.NodeId == nodeId);
        if (node == null) return NotFound(new { error = "Node not found" });

        node.Status = status;
        node.LastUpdated = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Node {nodeId} status updated to {status}" });
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

    [HttpPost("resolve-outage/{id}")]
    public async Task<IActionResult> ResolveOutage(int id)
    {
        var outage = await _db.Outages.FindAsync(id);
        if (outage == null) return NotFound(new { error = "Outage not found" });

        outage.ResolvedAt = DateTime.UtcNow;

        var node = await _db.NetworkNodes.FirstOrDefaultAsync(n => n.NodeId == outage.AffectedNodeId);
        if (node != null)
        {
            node.Status = NodeStatus.Healthy;
            node.LastUpdated = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Outage resolved successfully" });
    }
}

public record OutageRequest(string NodeId, string Reason, string Severity = "Critical");
public record AlertRequest(string Title, string Description, string Severity, string Region);
