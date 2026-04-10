using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.AssignWorklistItem;

public class AssignWorklistItemCommand(
    Guid analyzerSampleId,
    string examMnemonic,
    Guid worklistItemId,
    DateTime assignedAt
) : CommandBase
{
    public Guid AnalyzerSampleId { get; } = analyzerSampleId;
    public string ExamMnemonic { get; } = examMnemonic;
    public Guid WorklistItemId { get; } = worklistItemId;
    public DateTime AssignedAt { get; } = assignedAt;
}
