using System.ComponentModel.DataAnnotations;

namespace TelcoNet.Data.Entities;

public enum NodeStatus
{
    Healthy,
    Degraded,
    Down
}

public class NetworkNode
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string NodeId { get; set; } = string.Empty; // e.g., "LW-017"

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., "Lagos West Tower 17"

    [Required, MaxLength(100)]
    public string Region { get; set; } = string.Empty; // e.g., "Lagos West"

    [Required, MaxLength(100)]
    public string Zone { get; set; } = string.Empty; // e.g., "South West"

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public NodeStatus Status { get; set; } = NodeStatus.Healthy;

    [MaxLength(50)]
    public string NodeType { get; set; } = "Tower"; // Tower, Hub, Exchange

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
