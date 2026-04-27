namespace TelcoNet.Core.Models.DTOs;

// ── Network DTOs ──

public class NetworkNodeDto
{
    public string NodeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string Status { get; set; } = string.Empty; // "healthy", "degraded", "failed"
    public string NodeType { get; set; } = string.Empty;
}

public class RegionNodesDto
{
    public string Region { get; set; } = string.Empty;
    public List<NetworkNodeDto> Nodes { get; set; } = new();
}

public class NetworkHealthSummaryDto
{
    public string OverallStatus { get; set; } = "Healthy"; // Healthy, Degraded, Critical
    public int TotalNodes { get; set; }
    public int HealthyNodes { get; set; }
    public int DegradedNodes { get; set; }
    public int DownNodes { get; set; }
    public int ActiveOutages { get; set; }
}

// ── Outage DTOs ──

public class OutageDto
{
    public int Id { get; set; }
    public string Region { get; set; } = string.Empty;
    public string? AffectedNodeId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool IsActive { get; set; }
    public int EstimatedUsersAffected { get; set; }
}

// ── Alert DTOs ──

public class AlertDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? Region { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsAcknowledged { get; set; }
}

public class IncidentTimelineDto
{
    public string Time { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}

// ── User Management DTOs ──

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateRoleDto
{
    public string Role { get; set; } = string.Empty;
}
