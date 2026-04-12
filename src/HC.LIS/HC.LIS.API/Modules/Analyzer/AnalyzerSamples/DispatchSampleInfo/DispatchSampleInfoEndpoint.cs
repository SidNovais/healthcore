using HC.Core.Domain;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.DispatchSampleInfo;
using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.API.Modules.Analyzer.AnalyzerSamples.DispatchSampleInfo;

internal static class DispatchSampleInfoEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        IAnalyzerModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(
            new DispatchSampleInfoCommand(id, SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
