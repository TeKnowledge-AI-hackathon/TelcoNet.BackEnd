using System.ComponentModel;
using Microsoft.SemanticKernel;
using TelcoNet.Core.Interfaces;

namespace TelcoNet.Plugins;

/// <summary>
/// Plugin for retrieving alerts and notifications about network issues.
/// The AI calls this when users ask about alerts, warnings, or critical issues.
/// </summary>
public class AlertPlugin
{
    private readonly INetworkService _networkService;

    public AlertPlugin(INetworkService networkService)
    {
        _networkService = networkService;
    }

    [KernelFunction("get_alerts")]
    [Description("Gets current alerts and notifications about network issues. Returns recent alerts with severity levels and descriptions. Use when the user asks about alerts, warnings, critical issues, notifications, or what's happening on the network.")]
    public async Task<string> GetAlerts(
        [Description("Optional severity filter: 'Critical', 'High', 'Warning', 'Info', or 'Resolved'. Leave empty for all alerts.")] string? severity = null)
    {
        var alerts = await _networkService.GetAlertsAsync(severity);
        return System.Text.Json.JsonSerializer.Serialize(alerts);
    }
}
