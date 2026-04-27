namespace TelcoNet.Core.Models.DTOs;

// ── Dashboard & Analytics DTOs ──

public class KpiValueDto
{
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double ChangePercent { get; set; } // vs previous period
    public string Scope { get; set; } = string.Empty; // "Network-wide", "Lagos West", "Peak today"
}

public class DashboardKpisDto
{
    public KpiValueDto AvgLatency { get; set; } = new();
    public KpiValueDto PacketLoss { get; set; } = new();
    public KpiValueDto Throughput { get; set; } = new();
    public KpiValueDto ActiveUsers { get; set; } = new();
}

public class ChartSeriesDto
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public List<double> Data { get; set; } = new();
}

public class ChartDataDto
{
    public List<string> Labels { get; set; } = new();
    public List<ChartSeriesDto> Series { get; set; } = new();
}
