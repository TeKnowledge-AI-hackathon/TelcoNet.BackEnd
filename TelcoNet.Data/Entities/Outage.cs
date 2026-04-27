using System.ComponentModel.DataAnnotations;

namespace TelcoNet.Data.Entities;

public class Outage
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Region { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? AffectedNodeId { get; set; } // e.g., "LW-099"

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Severity { get; set; } = "Major"; // Critical, Major, Minor

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }

    public bool IsActive => ResolvedAt == null;

    public int EstimatedUsersAffected { get; set; }
}
