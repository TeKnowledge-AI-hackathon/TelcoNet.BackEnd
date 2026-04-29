using TelcoNet.Core.Models.DTOs;

namespace TelcoNet.Core.Interfaces;

public interface INetworkService
{
    Task<NetworkHealthSummaryDto> GetHealthSummaryAsync();
    Task<List<NetworkNodeDto>> GetNodesAsync(string? region = null, string? timeRange = "1h");
    Task<List<OutageDto>> GetOutagesAsync(string? region = null, bool activeOnly = true);
    Task<List<AlertDto>> GetAlertsAsync(string? severity = null, string? region = null);
    Task<List<IncidentTimelineDto>> GetIncidentTimelineAsync(string? region = null);
    Task<bool> AcknowledgeAlertAsync(int alertId);

    // Data access for plugins
    Task<string> GetRegionStatusAsync(string region);
    Task<string> GetMetricsAsync(string region, string timeRange = "24h");
    Task<string> GetActiveOutagesTextAsync(string? region = null);
    Task<string> GetBestCoverageAsync(string area);
}
