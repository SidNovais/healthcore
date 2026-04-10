using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleDetails;

public class GetAnalyzerSampleDetailsQuery(Guid analyzerSampleId) : QueryBase<AnalyzerSampleDetailsDto?>
{
    public Guid AnalyzerSampleId { get; } = analyzerSampleId;
}
