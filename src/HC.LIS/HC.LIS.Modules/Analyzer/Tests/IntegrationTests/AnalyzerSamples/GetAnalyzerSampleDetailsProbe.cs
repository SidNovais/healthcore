using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleDetails;
using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Modules.Analyzer.IntegrationTests.AnalyzerSamples;

public class GetAnalyzerSampleDetailsProbe(
    Guid analyzerSampleId,
    IAnalyzerModule analyzerModule,
    Func<AnalyzerSampleDetailsDto?, bool>? satisfiedWhen = null
) : IProbe<AnalyzerSampleDetailsDto>
{
    private readonly Guid _analyzerSampleId = analyzerSampleId;
    private readonly IAnalyzerModule _analyzerModule = analyzerModule;
    private readonly Func<AnalyzerSampleDetailsDto?, bool> _satisfiedWhen =
        satisfiedWhen ?? (dto => dto is not null);

    public string DescribeFailureTo() =>
        $"AnalyzerSampleDetails not found or unsatisfied for {_analyzerSampleId}";

    public async Task<AnalyzerSampleDetailsDto?> GetSampleAsync()
    {
        return await _analyzerModule
            .ExecuteQueryAsync(new GetAnalyzerSampleDetailsQuery(_analyzerSampleId))
            .ConfigureAwait(false);
    }

    public bool IsSatisfied(AnalyzerSampleDetailsDto? sample) => _satisfiedWhen(sample);
}
