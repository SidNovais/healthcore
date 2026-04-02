using System;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.RecordAnalysisResult;

public class RecordAnalysisResultCommand(
    Guid worklistItemId,
    string analyteCode,
    string resultValue,
    string resultUnit,
    string referenceRange,
    Guid performedById,
    DateTime recordedAt
) : CommandBase
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public string AnalyteCode { get; } = analyteCode;
    public string ResultValue { get; } = resultValue;
    public string ResultUnit { get; } = resultUnit;
    public string ReferenceRange { get; } = referenceRange;
    public Guid PerformedById { get; } = performedById;
    public DateTime RecordedAt { get; } = recordedAt;
}
