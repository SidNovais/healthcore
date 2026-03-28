using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;

public class WorklistItemCompletedDomainEvent(
    Guid worklistItemId,
    Guid sampleId,
    string examCode,
    string completionType,
    DateTime completedAt
) : DomainEvent
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public Guid SampleId { get; } = sampleId;
    public string ExamCode { get; } = examCode;
    public string CompletionType { get; } = completionType;
    public DateTime CompletedAt { get; } = completedAt;
}
