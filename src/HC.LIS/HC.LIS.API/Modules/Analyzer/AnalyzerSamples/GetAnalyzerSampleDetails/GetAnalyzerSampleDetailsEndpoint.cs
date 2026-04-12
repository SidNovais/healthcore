using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleDetails;
using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.API.Modules.Analyzer.AnalyzerSamples.GetAnalyzerSampleDetails;

internal static class GetAnalyzerSampleDetailsEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        IAnalyzerModule module,
        CancellationToken ct)
    {
        var result = await module.ExecuteQueryAsync(
            new GetAnalyzerSampleDetailsQuery(id)).ConfigureAwait(false);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}
