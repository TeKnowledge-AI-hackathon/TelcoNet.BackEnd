using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TelcoNet.Core.Interfaces;
using TelcoNet.Core.Models.DTOs;
using TelcoNet.Data;
using TelcoNet.Data.Entities;

namespace TelcoNet.Core.Services;

public class NetworkService : INetworkService
{
    private readonly AppDbContext _db;

    public NetworkService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<NetworkHealthSummaryDto> GetHealthSummaryAsync()
    {
        var nodes = await _db.NetworkNodes.ToListAsync();
        var activeOutages = await _db.Outages.CountAsync(o => o.ResolvedAt == null);

        var healthy = nodes.Count(n => n.Status == NodeStatus.Healthy);
        var degraded = nodes.Count(n => n.Status == NodeStatus.Degraded);
        var down = nodes.Count(n => n.Status == NodeStatus.Down);

        var overallStatus = down > 0 ? "Critical" : degraded > 0 ? "Degraded" : "Healthy";

        return new NetworkHealthSummaryDto
        {
            OverallStatus = overallStatus,
            TotalNodes = nodes.Count,
            HealthyNodes = healthy,
            DegradedNodes = degraded,
            DownNodes = down,
            ActiveOutages = activeOutages
        };
    }

    public async Task<List<NetworkNodeDto>> GetNodesAsync(string? region = null, string? timeRange = "1h")
    {
        var query = _db.NetworkNodes.AsQueryable();

        if (!string.IsNullOrEmpty(region))
            query = query.Where(n => n.Region.ToLower().Contains(region.ToLower()));

        var nodes = await query.ToListAsync();
        
        if (timeRange == "1h")
        {
            return nodes.Select(n => new NetworkNodeDto
            {
                NodeId = n.NodeId,
                Name = n.Name,
                Region = n.Region,
                Lat = n.Latitude,
                Lng = n.Longitude,
                Status = n.Status.ToString().ToLower(),
                NodeType = n.NodeType
            }).ToList();
        }

        // For historical views, check if nodes had outages/alerts in the timeframe
        var hours = timeRange == "7d" ? 168 : 24;
        var since = DateTime.UtcNow.AddHours(-hours);
        
        var historicalOutages = await _db.Outages
            .Where(o => o.StartedAt >= since || (o.ResolvedAt == null || o.ResolvedAt >= since))
            .ToListAsync();
            
        var historicalAlerts = await _db.Alerts
            .Where(a => a.CreatedAt >= since)
            .ToListAsync();

        return nodes.Select(n => {
            var status = n.Status.ToString().ToLower();
            
            // If node had a major outage in this window, show as down
            if (historicalOutages.Any(o => o.AffectedNodeId == n.NodeId))
                status = "down";
            else if (historicalAlerts.Any(a => a.Region == n.Region && a.Severity == "Warning"))
                status = "degraded";

            return new NetworkNodeDto
            {
                NodeId = n.NodeId,
                Name = n.Name,
                Region = n.Region,
                Lat = n.Latitude,
                Lng = n.Longitude,
                Status = status,
                NodeType = n.NodeType
            };
        }).ToList();
    }

    public async Task<List<OutageDto>> GetOutagesAsync(string? region = null, bool activeOnly = true)
    {
        var query = _db.Outages.AsQueryable();

        if (activeOnly)
            query = query.Where(o => o.ResolvedAt == null);

        if (!string.IsNullOrEmpty(region))
            query = query.Where(o => o.Region.ToLower().Contains(region.ToLower()));

        return await query.OrderByDescending(o => o.StartedAt)
            .Select(o => new OutageDto
            {
                Id = o.Id,
                Region = o.Region,
                AffectedNodeId = o.AffectedNodeId,
                Reason = o.Reason,
                Severity = o.Severity,
                StartedAt = o.StartedAt,
                ResolvedAt = o.ResolvedAt,
                IsActive = o.ResolvedAt == null,
                EstimatedUsersAffected = o.EstimatedUsersAffected
            }).ToListAsync();
    }

    public async Task<List<AlertDto>> GetAlertsAsync(string? severity = null, string? region = null)
    {
        var query = _db.Alerts.AsQueryable();

        if (!string.IsNullOrEmpty(severity))
            query = query.Where(a => a.Severity.ToLower() == severity.ToLower());

        if (!string.IsNullOrEmpty(region))
            query = query.Where(a => a.Region != null && a.Region.ToLower().Contains(region.ToLower()));

        return await query.OrderByDescending(a => a.CreatedAt)
            .Select(a => new AlertDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Severity = a.Severity,
                Region = a.Region,
                CreatedAt = a.CreatedAt,
                IsAcknowledged = a.IsAcknowledged
            }).ToListAsync();
    }

    public async Task<List<IncidentTimelineDto>> GetIncidentTimelineAsync(string? region = null)
    {
        var query = _db.Alerts.AsQueryable();

        if (!string.IsNullOrEmpty(region))
            query = query.Where(a => a.Region != null && a.Region.ToLower().Contains(region.ToLower()));

        return await query.OrderByDescending(a => a.CreatedAt)
            .Select(a => new IncidentTimelineDto
            {
                Time = a.CreatedAt.ToString("HH:mm"),
                Title = a.Title,
                Description = a.Description,
                Severity = a.Severity.ToLower()
            }).ToListAsync();
    }

    public async Task<bool> AcknowledgeAlertAsync(int alertId)
    {
        var alert = await _db.Alerts.FindAsync(alertId);
        if (alert == null) return false;

        alert.IsAcknowledged = true;
        alert.AcknowledgedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Plugin-facing methods (return text for AI) ──

    public async Task<string> GetRegionStatusAsync(string region)
    {
        var nodes = await GetNodesAsync(region);
        var metrics = await _db.NetworkMetrics
            .Where(m => m.Region.ToLower().Contains(region.ToLower()))
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefaultAsync();

        if (!nodes.Any())
            return $"No network nodes found for region '{region}'.";

        var result = new
        {
            Region = region,
            Nodes = nodes,
            LatestMetrics = metrics != null ? new
            {
                metrics.LatencyMs,
                metrics.ThroughputMbps,
                metrics.PacketLossPercent,
                metrics.UptimePercent,
                metrics.ActiveUsers,
                metrics.Timestamp
            } : null
        };

        return JsonSerializer.Serialize(result);
    }

    public async Task<string> GetMetricsAsync(string region, string timeRange = "24h")
    {
        var hours = timeRange switch
        {
            "1h" => 1,
            "7d" => 168,
            _ => 24
        };

        var since = DateTime.UtcNow.AddHours(-hours);
        var metrics = await _db.NetworkMetrics
            .Where(m => m.Region.ToLower().Contains(region.ToLower()) && m.Timestamp >= since)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        if (!metrics.Any())
            return $"No metrics found for region '{region}' in the last {timeRange}.";

        var avgLatency = metrics.Average(m => m.LatencyMs);
        var avgThroughput = metrics.Average(m => m.ThroughputMbps);
        var avgPacketLoss = metrics.Average(m => m.PacketLossPercent);
        var avgUptime = metrics.Average(m => m.UptimePercent);
        var peakUsers = metrics.Max(m => m.ActiveUsers);

        var result = new
        {
            Region = region,
            TimeRange = timeRange,
            DataPoints = metrics.Count,
            Summary = new
            {
                AvgLatencyMs = Math.Round(avgLatency, 1),
                AvgThroughputMbps = Math.Round(avgThroughput, 1),
                AvgPacketLossPercent = Math.Round(avgPacketLoss, 2),
                AvgUptimePercent = Math.Round(avgUptime, 2),
                PeakActiveUsers = peakUsers
            }
        };

        return JsonSerializer.Serialize(result);
    }

    public async Task<string> GetActiveOutagesTextAsync(string? region = null)
    {
        var outages = await GetOutagesAsync(region, activeOnly: true);

        if (!outages.Any())
            return region != null
                ? $"No active outages in {region}. All systems operational."
                : "No active outages detected across all regions.";

        return JsonSerializer.Serialize(outages);
    }

    public async Task<string> GetBestCoverageAsync(string area)
    {
        var nodes = await _db.NetworkNodes
            .Where(n => n.Region.ToLower().Contains(area.ToLower()) && n.Status == NodeStatus.Healthy)
            .ToListAsync();

        if (!nodes.Any())
        {
            // Check if the area exists but has no healthy nodes
            var anyNodes = await _db.NetworkNodes
                .AnyAsync(n => n.Region.ToLower().Contains(area.ToLower()));

            if (anyNodes)
                return $"No healthy towers found in {area}. The area may be experiencing outages. Try nearby regions.";

            return $"No network infrastructure found for area '{area}'. Try searching with a different area name like 'Lagos Island', 'Victoria Island', 'Ikeja', 'Abuja', etc.";
        }

        // Get latest metrics for the area
        var metrics = await _db.NetworkMetrics
            .Where(m => m.Region.ToLower().Contains(area.ToLower()))
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefaultAsync();

        var result = new
        {
            Area = area,
            HealthyTowers = nodes.Count,
            BestSpots = nodes.Select(n => new
            {
                n.Name,
                n.NodeId,
                Location = new { n.Latitude, n.Longitude },
                n.NodeType
            }),
            SignalQuality = metrics?.SignalStrengthDbm > -65 ? "Excellent" : metrics?.SignalStrengthDbm > -80 ? "Good" : "Fair",
            CurrentLatency = metrics != null ? $"{metrics.LatencyMs:F0}ms" : "N/A"
        };

        return JsonSerializer.Serialize(result);
    }
}
