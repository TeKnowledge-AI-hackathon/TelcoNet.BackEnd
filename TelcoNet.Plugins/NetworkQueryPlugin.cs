using System.ComponentModel;
using Microsoft.SemanticKernel;
using TelcoNet.Core.Interfaces;

namespace TelcoNet.Plugins;

/// <summary>
/// Plugin for querying network status and performance metrics.
/// The AI calls these functions when users ask about network health, speed, latency, etc.
/// </summary>
public class NetworkQueryPlugin
{
    private readonly INetworkService _networkService;

    public NetworkQueryPlugin(INetworkService networkService)
    {
        _networkService = networkService;
    }

    [KernelFunction("get_network_status")]
    [Description("Gets the current network status for a specific region or zone, including tower health and latest performance metrics. Use when a user asks about network performance, speed, latency, or health in an area.")]
    public async Task<string> GetNetworkStatus(
        [Description("The region name, e.g. 'Lagos West', 'Victoria Island', 'Abuja Central', 'Port Harcourt'")] string region)
    {
        return await _networkService.GetRegionStatusAsync(region);
    }

    [KernelFunction("get_network_metrics")]
    [Description("Gets detailed network performance metrics (latency, throughput, packet loss, uptime) for a region over a specified time range. Use when the user asks WHY something is slow, wants trend data, or asks for specific numbers.")]
    public async Task<string> GetNetworkMetrics(
        [Description("The region name")] string region,
        [Description("Time range: '1h' for last hour, '24h' for last 24 hours, '7d' for last 7 days. Default is '24h'.")] string timeRange = "24h")
    {
        return await _networkService.GetMetricsAsync(region, timeRange);
    }
}
