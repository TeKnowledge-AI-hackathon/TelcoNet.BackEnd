using System.ComponentModel.DataAnnotations;

namespace TelcoNet.Data.Entities;

public class Alert
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Severity { get; set; } = "Info"; // Critical, High, Warning, Info, Resolved

    [MaxLength(100)]
    public string? Region { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsAcknowledged { get; set; } = false;

    public DateTime? AcknowledgedAt { get; set; }
}
