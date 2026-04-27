using TelcoNet.Core.Models.DTOs;

namespace TelcoNet.Core.Interfaces;

public interface IDashboardService
{
    Task<DashboardKpisDto> GetKpisAsync(string? region = null);
    Task<ChartDataDto> GetLatencyChartAsync(string timeRange = "24h", string? regions = null);
    Task<ChartDataDto> GetThroughputChartAsync(string timeRange = "24h", string? region = null);
    Task<string> GetKpisTextAsync(string? region = null); // For plugin
}
