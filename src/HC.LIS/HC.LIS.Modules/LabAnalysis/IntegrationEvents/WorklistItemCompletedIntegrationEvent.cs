using System;
using HC.COre.Infrastructure.EventBus;

namespace HC.LIS.Modules.LabAnalysis.IntegrationEvents;

public class WorklistItemCompletedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid worklistItemId,
    Guid sampleId,
    string examCode,
    string completionType
) : IntegrationEvent(id, occurredAt)
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public Guid SampleId { get; } = sampleId;
    public string ExamCode { get; } = examCode;
    public string CompletionType { get; } = completionType;
}
