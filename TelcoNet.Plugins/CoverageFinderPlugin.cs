using System.ComponentModel;
using Microsoft.SemanticKernel;
using TelcoNet.Core.Interfaces;

namespace TelcoNet.Plugins;

/// <summary>
/// Plugin for finding the best spots for network connectivity.
/// The AI calls this when users ask about coverage, signal strength, or best spots.
/// </summary>
public class CoverageFinderPlugin
{
    private readonly INetworkService _networkService;

    public CoverageFinderPlugin(INetworkService networkService)
    {
        _networkService = networkService;
    }

    [KernelFunction("find_best_coverage")]
    [Description("Finds the best spots for network connectivity in a given area. Returns healthy towers, signal quality, and current performance. Use when the user asks about coverage, signal strength, connectivity, or best spots for network access.")]
    public async Task<string> FindBestCoverage(
        [Description("The area or region to search for coverage, e.g. 'Lagos Island', 'Victoria Island', 'Ikeja'")] string area)
    {
        return await _networkService.GetBestCoverageAsync(area);
    }
}
