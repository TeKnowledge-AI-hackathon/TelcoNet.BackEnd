using System.ComponentModel;
using Microsoft.SemanticKernel;
using TelcoNet.Core.Interfaces;

namespace TelcoNet.Plugins;

/// <summary>
/// Plugin for detecting and reporting on network outages.
/// The AI calls these functions when users ask about outages or if MTN is down.
/// </summary>
public class OutageDetectionPlugin
{
    private readonly INetworkService _networkService;

    public OutageDetectionPlugin(INetworkService networkService)
    {
        _networkService = networkService;
    }

    [KernelFunction("check_outages")]
    [Description("Checks for active network outages. Use when the user asks if the network is down, if there are issues, disruptions, or outages in a specific location or across the entire network.")]
    public async Task<string> CheckOutages(
        [Description("Optional region name to check. Leave empty to check all regions.")] string? region = null)
    {
        return await _networkService.GetActiveOutagesTextAsync(region);
    }
}
