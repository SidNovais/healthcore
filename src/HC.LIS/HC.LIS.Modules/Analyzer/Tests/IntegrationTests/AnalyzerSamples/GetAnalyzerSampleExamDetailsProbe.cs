using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleExamDetails;
using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Modules.Analyzer.IntegrationTests.AnalyzerSamples;

public class GetAnalyzerSampleExamDetailsProbe(
    Guid analyzerSampleId,
    IAnalyzerModule analyzerModule,
    Func<IReadOnlyCollection<AnalyzerSampleExamDetailsDto>?, bool>? satisfiedWhen = null
) : IProbe<IReadOnlyCollection<AnalyzerSampleExamDetailsDto>>
{
    private readonly Guid _analyzerSampleId = analyzerSampleId;
    private readonly IAnalyzerModule _analyzerModule = analyzerModule;
    private readonly Func<IReadOnlyCollection<AnalyzerSampleExamDetailsDto>?, bool> _satisfiedWhen =
        satisfiedWhen ?? (dtos => dtos is { Count: > 0 });

    public string DescribeFailureTo() =>
        $"AnalyzerSampleExamDetails not found or unsatisfied for {_analyzerSampleId}";

    public async Task<IReadOnlyCollection<AnalyzerSampleExamDetailsDto>?> GetSampleAsync()
    {
        return await _analyzerModule
            .ExecuteQueryAsync(new GetAnalyzerSampleExamDetailsQuery(_analyzerSampleId))
            .ConfigureAwait(false);
    }

    public bool IsSatisfied(IReadOnlyCollection<AnalyzerSampleExamDetailsDto>? sample) =>
        _satisfiedWhen(sample);
}
