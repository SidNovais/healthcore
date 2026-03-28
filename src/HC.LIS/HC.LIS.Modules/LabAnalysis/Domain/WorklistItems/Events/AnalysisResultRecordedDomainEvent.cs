using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;

public class AnalysisResultRecordedDomainEvent(
    Guid worklistItemId,
    string resultValue,
    Guid analystId,
    DateTime recordedAt
) : DomainEvent
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public string ResultValue { get; } = resultValue;
    public Guid AnalystId { get; } = analystId;
    public DateTime RecordedAt { get; } = recordedAt;
}
