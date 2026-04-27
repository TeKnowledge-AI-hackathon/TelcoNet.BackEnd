using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TelcoNet.Core.Interfaces;
using TelcoNet.Core.Models.DTOs;
using TelcoNet.Data;

namespace TelcoNet.Core.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    private static readonly Dictionary<string, string> RegionColors = new()
    {
        ["Lagos West"] = "#f59e0b",
        ["Lagos Island"] = "#6366f1",
        ["Victoria Island"] = "#ef4444",
        ["Ikeja"] = "#10b981",
        ["Abuja Central"] = "#3b82f6",
        ["Port Harcourt"] = "#f43f5e",
        ["Kano Metro"] = "#8b5cf6",
        ["Enugu"] = "#14b8a6"
    };

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardKpisDto> GetKpisAsync(string? region = null)
    {
        var query = _db.NetworkMetrics.AsQueryable();

        if (!string.IsNullOrEmpty(region))
            query = query.Where(m => m.Region.ToLower().Contains(region.ToLower()));

        // Get latest metrics
        var latestMetrics = await query
            .OrderByDescending(m => m.Timestamp)
            .GroupBy(m => m.Region)
            .Select(g => g.First())
            .ToListAsync();

        if (!latestMetrics.Any())
            return new DashboardKpisDto();

        // Get metrics from 24h ago for comparison
        var yesterday = DateTime.UtcNow.AddHours(-24);
        var oldMetrics = await query
            .Where(m => m.Timestamp <= yesterday)
            .OrderByDescending(m => m.Timestamp)
            .GroupBy(m => m.Region)
            .Select(g => g.First())
            .ToListAsync();

        var currentAvgLatency = latestMetrics.Average(m => m.LatencyMs);
        var currentAvgPacketLoss = latestMetrics.Average(m => m.PacketLossPercent);
        var currentMaxThroughput = latestMetrics.Max(m => m.ThroughputMbps);
        var currentTotalUsers = latestMetrics.Sum(m => m.ActiveUsers);

        var oldAvgLatency = oldMetrics.Any() ? oldMetrics.Average(m => m.LatencyMs) : currentAvgLatency;
        var oldAvgPacketLoss = oldMetrics.Any() ? oldMetrics.Average(m => m.PacketLossPercent) : currentAvgPacketLoss;
        var oldMaxThroughput = oldMetrics.Any() ? oldMetrics.Max(m => m.ThroughputMbps) : currentMaxThroughput;
        var oldTotalUsers = oldMetrics.Any() ? oldMetrics.Sum(m => m.ActiveUsers) : currentTotalUsers;

        // Find the worst latency region for the scope label
        var worstLatencyRegion = latestMetrics.OrderByDescending(m => m.LatencyMs).First().Region;

        return new DashboardKpisDto
        {
            AvgLatency = new KpiValueDto
            {
                Value = Math.Round(currentAvgLatency, 0),
                Unit = "ms",
                ChangePercent = CalculateChangePercent(currentAvgLatency, oldAvgLatency),
                Scope = !string.IsNullOrEmpty(region) ? region : worstLatencyRegion
            },
            PacketLoss = new KpiValueDto
            {
                Value = Math.Round(currentAvgPacketLoss, 1),
                Unit = "%",
                ChangePercent = CalculateChangePercent(currentAvgPacketLoss, oldAvgPacketLoss),
                Scope = !string.IsNullOrEmpty(region) ? region : "Network-wide"
            },
            Throughput = new KpiValueDto
            {
                Value = Math.Round(currentMaxThroughput / 1000, 1), // Convert Mbps to Gbps
                Unit = "Gbps",
                ChangePercent = CalculateChangePercent(currentMaxThroughput, oldMaxThroughput),
                Scope = "Peak today"
            },
            ActiveUsers = new KpiValueDto
            {
                Value = currentTotalUsers,
                Unit = "K",
                ChangePercent = CalculateChangePercent(currentTotalUsers, oldTotalUsers),
                Scope = "Current"
            }
        };
    }

    public async Task<ChartDataDto> GetLatencyChartAsync(string timeRange = "24h", string? regions = null)
    {
        var hours = timeRange switch { "1h" => 1, "7d" => 168, _ => 24 };
        var since = DateTime.UtcNow.AddHours(-hours);

        // Determine which regions to include
        List<string> regionList;
        if (!string.IsNullOrEmpty(regions))
        {
            regionList = regions.Split(',').Select(r => r.Trim()).ToList();
        }
        else
        {
            // Default: show the most interesting regions
            regionList = new List<string> { "Lagos West", "Lagos Island", "Ikeja", "Victoria Island" };
        }

        var metrics = await _db.NetworkMetrics
            .Where(m => regionList.Contains(m.Region) && m.Timestamp >= since)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        var grouped = metrics.GroupBy(m => m.Region);

        var labels = metrics
            .Select(m => m.Timestamp.ToString("HH:mm"))
            .Distinct()
            .ToList();

        var series = grouped.Select(g => new ChartSeriesDto
        {
            Name = g.Key,
            Color = RegionColors.GetValueOrDefault(g.Key, "#94a3b8"),
            Data = g.Select(m => Math.Round(m.LatencyMs, 1)).ToList()
        }).ToList();

        return new ChartDataDto { Labels = labels, Series = series };
    }

    public async Task<ChartDataDto> GetThroughputChartAsync(string timeRange = "24h", string? region = null)
    {
        var hours = timeRange switch { "1h" => 1, "7d" => 168, _ => 24 };
        var since = DateTime.UtcNow.AddHours(-hours);

        var query = _db.NetworkMetrics
            .Where(m => m.Timestamp >= since);

        if (!string.IsNullOrEmpty(region))
            query = query.Where(m => m.Region.ToLower().Contains(region.ToLower()));

        var metrics = await query
            .OrderBy(m => m.Timestamp)
            .GroupBy(m => m.Timestamp)
            .Select(g => new { Time = g.Key, AvgThroughput = g.Average(m => m.ThroughputMbps) })
            .ToListAsync();

        return new ChartDataDto
        {
            Labels = metrics.Select(m => m.Time.ToString("HH:mm")).ToList(),
            Series = new List<ChartSeriesDto>
            {
                new()
                {
                    Name = region ?? "Network-wide",
                    Color = "#3b82f6",
                    Data = metrics.Select(m => Math.Round(m.AvgThroughput / 1000, 2)).ToList() // Gbps
                }
            }
        };
    }

    public async Task<string> GetKpisTextAsync(string? region = null)
    {
        var kpis = await GetKpisAsync(region);
        return JsonSerializer.Serialize(kpis);
    }

    private static double CalculateChangePercent(double current, double previous)
    {
        if (previous == 0) return 0;
        return Math.Round((current - previous) / previous * 100, 1);
    }
}
