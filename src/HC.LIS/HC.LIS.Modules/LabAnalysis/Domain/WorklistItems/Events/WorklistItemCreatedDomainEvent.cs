using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;

public class WorklistItemCreatedDomainEvent(
    Guid worklistItemId,
    Guid sampleId,
    string sampleBarcode,
    string examCode,
    Guid patientId,
    Guid orderId,
    Guid orderItemId,
    DateTime createdAt
) : DomainEvent
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public Guid SampleId { get; } = sampleId;
    public string SampleBarcode { get; } = sampleBarcode;
    public string ExamCode { get; } = examCode;
    public Guid PatientId { get; } = patientId;
    public Guid OrderId { get; } = orderId;
    public Guid OrderItemId { get; } = orderItemId;
    public DateTime CreatedAt { get; } = createdAt;
}
