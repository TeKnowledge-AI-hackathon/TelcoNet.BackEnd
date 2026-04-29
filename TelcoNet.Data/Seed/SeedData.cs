using TelcoNet.Data.Entities;

namespace TelcoNet.Data.Seed;

public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        // Only seed if empty
        if (context.Users.Any()) return;

        SeedUsers(context);
        SeedNetworkNodes(context);
        SeedNetworkMetrics(context);
        SeedOutages(context);
        SeedAlerts(context);
        SeedSystemSettings(context);

        context.SaveChanges();
    }

    private static void SeedUsers(AppDbContext context)
    {
        context.Users.AddRange(
            new User
            {
                FullName = "Admin User",
                Email = "admin@noc.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin
            },
            new User
            {
                FullName = "NOC Operator",
                Email = "operator@noc.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Operator@123"),
                Role = UserRole.Operator
            },
            new User
            {
                FullName = "Read-Only Viewer",
                Email = "viewer@noc.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Viewer@123"),
                Role = UserRole.Viewer
            },
            new User
            {
                FullName = "John Smith",
                Email = "john.smith@noc.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Operator@123"),
                Role = UserRole.Operator
            },
            new User
            {
                FullName = "Sarah Johnson",
                Email = "sarah.j@noc.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Viewer@123"),
                Role = UserRole.Viewer,
                IsActive = false // Inactive user as shown in design
            }
        );
    }

    private static void SeedNetworkNodes(AppDbContext context)
    {
        context.NetworkNodes.AddRange(
            // Lagos West — Degraded zone
            new NetworkNode { NodeId = "LW-001", Name = "Lagos West Hub", Region = "Lagos West", Zone = "South West", Latitude = 6.4541, Longitude = 3.3947, Status = NodeStatus.Degraded, NodeType = "Hub" },
            new NetworkNode { NodeId = "LW-017", Name = "Lagos West Tower 17", Region = "Lagos West", Zone = "South West", Latitude = 6.4412, Longitude = 3.3815, Status = NodeStatus.Degraded, NodeType = "Tower" },
            new NetworkNode { NodeId = "LW-099", Name = "Lagos West Tower 99", Region = "Lagos West", Zone = "South West", Latitude = 6.4290, Longitude = 3.3520, Status = NodeStatus.Down, NodeType = "Tower" },

            // Lagos Island — Healthy zone
            new NetworkNode { NodeId = "LI-001", Name = "Lagos Island Hub", Region = "Lagos Island", Zone = "South West", Latitude = 6.4550, Longitude = 3.4200, Status = NodeStatus.Healthy, NodeType = "Hub" },
            new NetworkNode { NodeId = "LI-003", Name = "Lagos Island Tower 3", Region = "Lagos Island", Zone = "South West", Latitude = 6.4480, Longitude = 3.4100, Status = NodeStatus.Healthy, NodeType = "Tower" },

            // Victoria Island — Healthy
            new NetworkNode { NodeId = "VI-001", Name = "Victoria Island Hub", Region = "Victoria Island", Zone = "South West", Latitude = 6.4281, Longitude = 3.4219, Status = NodeStatus.Healthy, NodeType = "Hub" },
            new NetworkNode { NodeId = "VI-005", Name = "Victoria Island Tower 5", Region = "Victoria Island", Zone = "South West", Latitude = 6.4315, Longitude = 3.4310, Status = NodeStatus.Healthy, NodeType = "Tower" },

            // Ikeja — Healthy
            new NetworkNode { NodeId = "IK-001", Name = "Ikeja Hub", Region = "Ikeja", Zone = "South West", Latitude = 6.6018, Longitude = 3.3515, Status = NodeStatus.Healthy, NodeType = "Hub" },
            new NetworkNode { NodeId = "IK-012", Name = "Ikeja Tower 12", Region = "Ikeja", Zone = "South West", Latitude = 6.5950, Longitude = 3.3480, Status = NodeStatus.Healthy, NodeType = "Tower" },

            // Abuja — Healthy
            new NetworkNode { NodeId = "AB-001", Name = "Abuja Central Hub", Region = "Abuja Central", Zone = "North Central", Latitude = 9.0579, Longitude = 7.4951, Status = NodeStatus.Healthy, NodeType = "Hub" },
            new NetworkNode { NodeId = "AB-015", Name = "Abuja Tower 15", Region = "Abuja Central", Zone = "North Central", Latitude = 9.0650, Longitude = 7.5050, Status = NodeStatus.Healthy, NodeType = "Tower" },

            // Port Harcourt — Down
            new NetworkNode { NodeId = "PH-001", Name = "Port Harcourt Hub", Region = "Port Harcourt", Zone = "South South", Latitude = 4.8156, Longitude = 7.0498, Status = NodeStatus.Down, NodeType = "Hub" },
            new NetworkNode { NodeId = "PH-004", Name = "Port Harcourt Tower 4", Region = "Port Harcourt", Zone = "South South", Latitude = 4.8200, Longitude = 7.0350, Status = NodeStatus.Down, NodeType = "Tower" },

            // Kano — Healthy
            new NetworkNode { NodeId = "KN-001", Name = "Kano Metro Hub", Region = "Kano Metro", Zone = "North West", Latitude = 12.0022, Longitude = 8.5919, Status = NodeStatus.Healthy, NodeType = "Hub" },

            // Enugu — Degraded
            new NetworkNode { NodeId = "EN-001", Name = "Enugu Hub", Region = "Enugu", Zone = "South East", Latitude = 6.4414, Longitude = 7.4985, Status = NodeStatus.Degraded, NodeType = "Hub" },

            // Jos South (Plateau) — Healthy (Newly Added)
            new NetworkNode { NodeId = "JS-001", Name = "Jos South Hub", Region = "Jos South", Zone = "North Central", Latitude = 9.8000, Longitude = 8.8500, Status = NodeStatus.Healthy, NodeType = "Hub" },
            new NetworkNode { NodeId = "JS-012", Name = "Rayfield Tower 12", Region = "Jos South", Zone = "North Central", Latitude = 9.8150, Longitude = 8.8720, Status = NodeStatus.Healthy, NodeType = "Tower" },
            new NetworkNode { NodeId = "JS-005", Name = "Bukuru Tower 05", Region = "Jos South", Zone = "North Central", Latitude = 9.7820, Longitude = 8.8410, Status = NodeStatus.Healthy, NodeType = "Tower" },

            // Ibadan (Oyo) — Degraded due to congestion
            new NetworkNode { NodeId = "IB-001", Name = "Ibadan Central Hub", Region = "Ibadan", Zone = "South West", Latitude = 7.3775, Longitude = 3.9470, Status = NodeStatus.Degraded, NodeType = "Hub" },
            new NetworkNode { NodeId = "IB-022", Name = "Bodija Tower 22", Region = "Ibadan", Zone = "South West", Latitude = 7.4100, Longitude = 3.9200, Status = NodeStatus.Degraded, NodeType = "Tower" },

            // Kaduna Central — Healthy
            new NetworkNode { NodeId = "KD-001", Name = "Kaduna Central Hub", Region = "Kaduna Central", Zone = "North West", Latitude = 10.5105, Longitude = 7.4165, Status = NodeStatus.Healthy, NodeType = "Hub" },

            // Benin City (Edo) — Critical Outage
            new NetworkNode { NodeId = "BN-001", Name = "Benin Hub", Region = "Benin City", Zone = "South South", Latitude = 6.3350, Longitude = 5.6037, Status = NodeStatus.Down, NodeType = "Hub" },
            new NetworkNode { NodeId = "BN-009", Name = "Uselu Tower 09", Region = "Benin City", Zone = "South South", Latitude = 6.3510, Longitude = 5.6120, Status = NodeStatus.Down, NodeType = "Tower" },

            // Warri (Delta) — Warning
            new NetworkNode { NodeId = "WR-001", Name = "Warri Port Hub", Region = "Warri", Zone = "South South", Latitude = 5.5167, Longitude = 5.7500, Status = NodeStatus.Degraded, NodeType = "Hub" }
        );
    }

    private static void SeedNetworkMetrics(AppDbContext context)
    {
        var now = DateTime.UtcNow;
        var regions = new[] { 
            "Lagos West", "Lagos Island", "Victoria Island", "Ikeja", 
            "Abuja Central", "Port Harcourt", "Kano Metro", "Enugu",
            "Jos South", "Ibadan", "Kaduna Central", "Benin City", "Warri" 
        };

        // Generate 24 hours of metrics for each region (hourly intervals)
        foreach (var region in regions)
        {
            for (int hoursAgo = 24; hoursAgo >= 0; hoursAgo--)
            {
                var timestamp = now.AddHours(-hoursAgo);
                var metric = GenerateMetric(region, timestamp, hoursAgo);
                context.NetworkMetrics.Add(metric);
            }
        }
    }

    private static NetworkMetric GenerateMetric(string region, DateTime timestamp, int hoursAgo)
    {
        var random = new Random(region.GetHashCode() + hoursAgo);

        return region switch
        {
            "Lagos West" => new NetworkMetric
            {
                Region = region,
                LatencyMs = 350 + random.Next(0, 200), // High latency (degraded)
                ThroughputMbps = 8 + random.NextDouble() * 10,
                PacketLossPercent = 5 + random.NextDouble() * 8,
                UptimePercent = 88 + random.NextDouble() * 7,
                ActiveUsers = 8000 + random.Next(0, 4000),
                SignalStrengthDbm = -85 + random.Next(0, 15),
                Timestamp = timestamp
            },
            "Port Harcourt" => new NetworkMetric
            {
                Region = region,
                LatencyMs = hoursAgo > 3 ? 45 + random.Next(0, 20) : 999, // Was fine, then went down
                ThroughputMbps = hoursAgo > 3 ? 60 + random.NextDouble() * 30 : 0,
                PacketLossPercent = hoursAgo > 3 ? random.NextDouble() * 2 : 100,
                UptimePercent = hoursAgo > 3 ? 98 + random.NextDouble() * 2 : 0,
                ActiveUsers = hoursAgo > 3 ? 5000 + random.Next(0, 3000) : 0,
                SignalStrengthDbm = hoursAgo > 3 ? -60 + random.Next(0, 10) : -120,
                Timestamp = timestamp
            },
            "Enugu" => new NetworkMetric
            {
                Region = region,
                LatencyMs = 120 + random.Next(0, 80),
                ThroughputMbps = 25 + random.NextDouble() * 20,
                PacketLossPercent = 2 + random.NextDouble() * 4,
                UptimePercent = 93 + random.NextDouble() * 5,
                ActiveUsers = 3000 + random.Next(0, 2000),
                SignalStrengthDbm = -75 + random.Next(0, 12),
                Timestamp = timestamp
            },
            "Benin City" => new NetworkMetric
            {
                Region = region,
                LatencyMs = hoursAgo > 5 ? 35 + random.Next(0, 15) : 1200,
                ThroughputMbps = hoursAgo > 5 ? 55 + random.NextDouble() * 20 : 0,
                PacketLossPercent = hoursAgo > 5 ? random.NextDouble() * 1.5 : 95,
                UptimePercent = hoursAgo > 5 ? 99 + random.NextDouble() * 1 : 0,
                ActiveUsers = hoursAgo > 5 ? 6000 + random.Next(0, 4000) : 0,
                SignalStrengthDbm = hoursAgo > 5 ? -55 + random.Next(0, 10) : -130,
                Timestamp = timestamp
            },
            "Ibadan" => new NetworkMetric
            {
                Region = region,
                LatencyMs = 210 + random.Next(0, 100), // Congested
                ThroughputMbps = 5 + random.NextDouble() * 5,
                PacketLossPercent = 8 + random.NextDouble() * 5,
                UptimePercent = 95 + random.NextDouble() * 4,
                ActiveUsers = 15000 + random.Next(0, 5000),
                SignalStrengthDbm = -80 + random.Next(0, 15),
                Timestamp = timestamp
            },
            _ => new NetworkMetric // Healthy regions
            {
                Region = region,
                LatencyMs = 15 + random.Next(0, 25),
                ThroughputMbps = 70 + random.NextDouble() * 30,
                PacketLossPercent = random.NextDouble() * 1.5,
                UptimePercent = 98.5 + random.NextDouble() * 1.5,
                ActiveUsers = 4000 + random.Next(0, 6000),
                SignalStrengthDbm = -50 + random.Next(0, 15),
                Timestamp = timestamp
            }
        };
    }

    private static void SeedOutages(AppDbContext context)
    {
        var now = DateTime.UtcNow;

        context.Outages.AddRange(
            new Outage
            {
                Region = "Port Harcourt",
                AffectedNodeId = "PH-004",
                Reason = "Fiber cut on trunk line PH-04 — construction crew damage",
                Severity = "Critical",
                StartedAt = now.AddHours(-3),
                EstimatedUsersAffected = 15000
            },
            new Outage
            {
                Region = "Benin City",
                AffectedNodeId = "BN-001",
                Reason = "Power outage at main distribution hub. Fuel theft from backup generator detected.",
                Severity = "Critical",
                StartedAt = now.AddHours(-5),
                EstimatedUsersAffected = 22000
            },
            new Outage
            {
                Region = "Ibadan",
                AffectedNodeId = "IB-001",
                Reason = "Severe congestion due to religious festival — capacity upgrade scheduled.",
                Severity = "Major",
                StartedAt = now.AddHours(-24),
                EstimatedUsersAffected = 12000
            },
            new Outage
            {
                Region = "Lagos West",
                AffectedNodeId = "LW-099",
                Reason = "Power supply failure during grid outage. Backup generator failed to start.",
                Severity = "Major",
                StartedAt = now.AddHours(-1.5),
                EstimatedUsersAffected = 8500
            },
            new Outage
            {
                Region = "Lagos West",
                AffectedNodeId = "LW-017",
                Reason = "Backhaul link congestion at 92% capacity",
                Severity = "Minor",
                StartedAt = now.AddHours(-2),
                EstimatedUsersAffected = 3200
            },
            // A resolved outage (for history)
            new Outage
            {
                Region = "Enugu",
                AffectedNodeId = "EN-001",
                Reason = "Brief power fluctuation causing microwave link instability",
                Severity = "Minor",
                StartedAt = now.AddDays(-1),
                ResolvedAt = now.AddDays(-1).AddHours(2),
                EstimatedUsersAffected = 2000
            }
        );
    }

    private static void SeedAlerts(AppDbContext context)
    {
        var now = DateTime.UtcNow;

        context.Alerts.AddRange(
            // Active, current alerts (matching the Timeline design)
            new Alert
            {
                Title = "Secondary backhaul activated",
                Description = "40% of traffic rerouted to backup fiber link",
                Severity = "Resolved",
                Region = "Lagos West",
                CreatedAt = now.AddMinutes(-15)
            },
            new Alert
            {
                Title = "Repair team dispatched",
                Description = "Field engineers en route to tower LW-099",
                Severity = "Info",
                Region = "Lagos West",
                CreatedAt = now.AddMinutes(-30)
            },
            new Alert
            {
                Title = "High latency alert triggered",
                Description = "Average latency exceeded 400ms threshold in Lagos West region",
                Severity = "High",
                Region = "Lagos West",
                CreatedAt = now.AddMinutes(-45)
            },
            new Alert
            {
                Title = "Congestion detected",
                Description = "Backhaul link capacity reached 92% — automatic alerts sent to NOC",
                Severity = "Warning",
                Region = "Lagos West",
                CreatedAt = now.AddMinutes(-75)
            },
            new Alert
            {
                Title = "Tower LW-099 offline",
                Description = "Power supply failure during grid outage. Backup generator failed to start.",
                Severity = "Critical",
                Region = "Lagos West",
                CreatedAt = now.AddMinutes(-100)
            },
            new Alert
            {
                Title = "Grid power outage detected",
                Description = "Brief power outage in Lagos West sector",
                Severity = "Critical",
                Region = "Lagos West",
                CreatedAt = now.AddMinutes(-105)
            },
            new Alert
            {
                Title = "Routine capacity check",
                Description = "All systems operating within normal parameters",
                Severity = "Info",
                Region = "Lagos West",
                CreatedAt = now.AddHours(-4)
            },
            // Port Harcourt alerts
            new Alert
            {
                Title = "Fiber cut detected — PH-04 trunk line",
                Description = "Construction crew accidentally severed main fiber line. Estimated 15,000 users affected.",
                Severity = "Critical",
                Region = "Port Harcourt",
                CreatedAt = now.AddHours(-3)
            },
            new Alert
            {
                Title = "Port Harcourt hub unreachable",
                Description = "All towers in PH region reporting no connectivity to hub PH-001",
                Severity = "Critical",
                Region = "Port Harcourt",
                CreatedAt = now.AddHours(-3).AddMinutes(5)
            },
            // Latency spike alert — Victoria Island
            new Alert
            {
                Title = "Latency spike in Victoria Island",
                Description = "Latency briefly exceeded normal thresholds. Monitoring situation.",
                Severity = "Warning",
                Region = "Victoria Island",
                CreatedAt = now.AddMinutes(-5),
                IsAcknowledged = true,
                AcknowledgedAt = now.AddMinutes(-2)
            }
        );
    }

    private static void SeedSystemSettings(AppDbContext context)
    {
        if (context.SystemSettings.Any()) return;

        context.SystemSettings.AddRange(
            new SystemSetting { Key = "RefreshRate", Value = "30", Group = "Dashboard", Description = "Data Refresh Rate (seconds)" },
            new SystemSetting { Key = "TwoFactorAuth", Value = "true", Group = "Security", Description = "Require 2FA for all users" },
            new SystemSetting { Key = "SessionTimeout", Value = "30", Group = "Security", Description = "Session Timeout (minutes)" },
            new SystemSetting { Key = "AuditLogging", Value = "true", Group = "Security", Description = "Log all user actions" },
            new SystemSetting { Key = "MetricsRetention", Value = "90", Group = "Data", Description = "Metrics Retention (days)" },
            new SystemSetting { Key = "AlertRetention", Value = "180", Group = "Data", Description = "Alert Log Retention (days)" }
        );
    }
}
