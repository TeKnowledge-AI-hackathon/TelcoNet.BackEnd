using System.ComponentModel.DataAnnotations;

namespace TelcoNet.Data.Entities;

public class NetworkMetric
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Region { get; set; } = string.Empty;

    /// <summary>Average latency in milliseconds</summary>
    public double LatencyMs { get; set; }

    /// <summary>Network throughput in Mbps</summary>
    public double ThroughputMbps { get; set; }

    /// <summary>Packet loss percentage</summary>
    public double PacketLossPercent { get; set; }

    /// <summary>Uptime percentage (0-100)</summary>
    public double UptimePercent { get; set; }

    /// <summary>Number of active users connected</summary>
    public int ActiveUsers { get; set; }

    /// <summary>Signal strength in dBm</summary>
    public double SignalStrengthDbm { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
