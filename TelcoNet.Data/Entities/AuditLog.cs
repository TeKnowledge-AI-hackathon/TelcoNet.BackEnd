using System.ComponentModel.DataAnnotations;

namespace TelcoNet.Data.Entities;

public class AuditLog
{
    [Key]
    public int Id { get; set; }

    [MaxLength(150)]
    public string? UserEmail { get; set; }

    [Required, MaxLength(10)]
    public string HttpMethod { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Endpoint { get; set; } = string.Empty;

    public int StatusCode { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    public long DurationMs { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
