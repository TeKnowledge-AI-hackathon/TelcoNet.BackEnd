using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelcoNet.Core.Interfaces;

namespace TelcoNet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>Get dashboard KPIs (the 4 metric cards).</summary>
    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis([FromQuery] string? region = null)
    {
        var kpis = await _dashboardService.GetKpisAsync(region);
        return Ok(kpis);
    }

    /// <summary>Get latency comparison chart data (multi-line chart).</summary>
    [HttpGet("charts/latency")]
    public async Task<IActionResult> GetLatencyChart(
        [FromQuery] string timeRange = "24h",
        [FromQuery] string? regions = null)
    {
        var chart = await _dashboardService.GetLatencyChartAsync(timeRange, regions);
        return Ok(chart);
    }

    /// <summary>Get network throughput chart data (area chart).</summary>
    [HttpGet("charts/throughput")]
    public async Task<IActionResult> GetThroughputChart(
        [FromQuery] string timeRange = "24h",
        [FromQuery] string? region = null)
    {
        var chart = await _dashboardService.GetThroughputChartAsync(timeRange, region);
        return Ok(chart);
    }
}
