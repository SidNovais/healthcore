using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;

public class AnalysisResultRecordedDomainEvent(
    Guid worklistItemId,
    string analyteCode,
    string resultValue,
    string resultUnit,
    string referenceRange,
    Guid performedById,
    DateTime recordedAt
) : DomainEvent
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public string AnalyteCode { get; } = analyteCode;
    public string ResultValue { get; } = resultValue;
    public string ResultUnit { get; } = resultUnit;
    public string ReferenceRange { get; } = referenceRange;
    public Guid PerformedById { get; } = performedById;
    public DateTime RecordedAt { get; } = recordedAt;
}
