using System.ComponentModel;
using Microsoft.SemanticKernel;
using TelcoNet.Core.Interfaces;

namespace TelcoNet.Plugins;

/// <summary>
/// Plugin for retrieving Key Performance Indicators (KPIs).
/// The AI calls this when users ask about overall performance, analytics, or dashboard data.
/// </summary>
public class KpiPlugin
{
    private readonly IDashboardService _dashboardService;

    public KpiPlugin(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [KernelFunction("get_kpis")]
    [Description("Gets Key Performance Indicators for the network including average latency, packet loss, throughput, and active users. Use when the user asks about overall performance, statistics, analytics, numbers, or dashboard data.")]
    public async Task<string> GetKpis(
        [Description("Optional region name to get KPIs for a specific region. Leave empty for network-wide KPIs.")] string? region = null)
    {
        return await _dashboardService.GetKpisTextAsync(region);
    }
}
