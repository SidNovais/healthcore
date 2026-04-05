using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;

public class WorklistItemCompletedDomainEvent(
    Guid worklistItemId,
    Guid sampleId,
    string examCode,
    string completionType,
    Guid orderId,
    Guid orderItemId,
    DateTime completedAt
) : DomainEvent
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public Guid SampleId { get; } = sampleId;
    public string ExamCode { get; } = examCode;
    public string CompletionType { get; } = completionType;
    public Guid OrderId { get; } = orderId;
    public Guid OrderItemId { get; } = orderItemId;
    public DateTime CompletedAt { get; } = completedAt;
}
