using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.DispatchSampleInfo;

public class DispatchSampleInfoCommand(
    Guid analyzerSampleId,
    DateTime dispatchedAt
) : CommandBase
{
    public Guid AnalyzerSampleId { get; } = analyzerSampleId;
    public DateTime DispatchedAt { get; } = dispatchedAt;
}
