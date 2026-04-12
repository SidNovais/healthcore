using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleExamDetails;
using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.API.Modules.Analyzer.AnalyzerSamples.GetAnalyzerSampleExamDetails;

internal static class GetAnalyzerSampleExamDetailsEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        IAnalyzerModule module,
        CancellationToken ct)
    {
        var result = await module.ExecuteQueryAsync(
            new GetAnalyzerSampleExamDetailsQuery(id)).ConfigureAwait(false);

        return TypedResults.Ok(result);
    }
}
