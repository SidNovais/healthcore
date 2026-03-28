using System;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.RecordAnalysisResult;

public class RecordAnalysisResultCommand(
    Guid worklistItemId,
    string resultValue,
    Guid analystId,
    DateTime recordedAt
) : CommandBase
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public string ResultValue { get; } = resultValue;
    public Guid AnalystId { get; } = analystId;
    public DateTime RecordedAt { get; } = recordedAt;
}
