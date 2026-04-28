using Microsoft.EntityFrameworkCore;
using TelcoNet.Data;
using TelcoNet.Data.Entities;

namespace TelcoNet.API.Services;

public class NetworkMonitoringWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NetworkMonitoringWorker> _logger;

    public NetworkMonitoringWorker(IServiceProvider serviceProvider, ILogger<NetworkMonitoringWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Network Monitoring Worker starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await SimulateMetricsAsync(dbContext, stoppingToken);
                await EvaluateThresholdsAsync(dbContext, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Network Monitoring Worker.");
            }

            // Wait 15 seconds before checking again
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }

        _logger.LogInformation("Network Monitoring Worker stopping.");
    }

    private async Task SimulateMetricsAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        // For demonstration, we pick a random region and generate a new metric.
        // In a real system, external sensors would be inserting these metrics.
        var regions = new[] { "Lagos West", "Lagos Island", "Victoria Island", "Ikeja", "Abuja Central", "Port Harcourt", "Kano Metro", "Enugu" };
        var random = new Random();
        var region = regions[random.Next(regions.Length)];

        // Sometimes trigger a high latency or congestion scenario
        bool triggerHighLatency = random.Next(100) < 15; // 15% chance
        bool triggerCongestion = random.Next(100) < 15; // 15% chance

        var metric = new NetworkMetric
        {
            Region = region,
            LatencyMs = triggerHighLatency ? random.Next(401, 800) : random.Next(15, 60),
            ThroughputMbps = triggerCongestion ? random.Next(2, 9) : random.Next(40, 100),
            PacketLossPercent = triggerHighLatency ? random.NextDouble() * 15 + 5 : random.NextDouble() * 2,
            UptimePercent = random.NextDouble() * 5 + 95,
            ActiveUsers = random.Next(3000, 10000),
            SignalStrengthDbm = random.Next(-90, -50),
            Timestamp = DateTime.UtcNow
        };

        db.NetworkMetrics.Add(metric);
        await db.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation($"Simulated new metric for {region}. Latency: {metric.LatencyMs}ms, Throughput: {metric.ThroughputMbps:F1}Mbps");
    }

    private async Task EvaluateThresholdsAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        // Get the latest metric for each region
        var latestMetrics = await db.NetworkMetrics
            .GroupBy(m => m.Region)
            .Select(g => g.OrderByDescending(m => m.Timestamp).FirstOrDefault())
            .ToListAsync(cancellationToken);

        foreach (var metric in latestMetrics)
        {
            if (metric == null) continue;

            // 1. Check Latency Threshold (> 400ms)
            if (metric.LatencyMs > 400)
            {
                await TriggerAlertAsync(db, metric.Region, "High latency alert triggered", 
                    $"Average latency exceeded 400ms threshold in {metric.Region} region ({metric.LatencyMs}ms)", "High");
            }

            // 2. Check Congestion Threshold (Throughput < 10Mbps)
            if (metric.ThroughputMbps < 10)
            {
                await TriggerAlertAsync(db, metric.Region, "Congestion detected", 
                    $"Throughput dropped to {metric.ThroughputMbps:F1}Mbps in {metric.Region}. Possible network congestion.", "Warning");
            }
        }
    }

    private async Task TriggerAlertAsync(AppDbContext db, string region, string title, string description, string severity)
    {
        // Check if there is already an active (unacknowledged) alert for this exact issue and region
        var existingAlert = await db.Alerts.FirstOrDefaultAsync(a => 
            a.Region == region && 
            a.Title == title && 
            !a.IsAcknowledged);

        if (existingAlert == null)
        {
            var alert = new Alert
            {
                Title = title,
                Description = description,
                Severity = severity,
                Region = region,
                CreatedAt = DateTime.UtcNow,
                IsAcknowledged = false
            };

            db.Alerts.Add(alert);
            await db.SaveChangesAsync();
            _logger.LogWarning($"Automated Alert Triggered: [{severity}] {title} in {region}");
        }
    }
}
