using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleExamDetails;

public class GetAnalyzerSampleExamDetailsQuery(Guid analyzerSampleId) : QueryBase<IReadOnlyCollection<AnalyzerSampleExamDetailsDto>>
{
    public Guid AnalyzerSampleId { get; } = analyzerSampleId;
}
