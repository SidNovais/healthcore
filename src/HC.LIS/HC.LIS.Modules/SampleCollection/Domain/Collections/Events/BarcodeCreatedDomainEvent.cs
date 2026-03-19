using System;
using System.Collections.Generic;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

public class BarcodeCreatedDomainEvent(
    Guid collectionRequestId,
    Guid sampleId,
    Guid patientId,
    Guid orderId,
    string barcodeValue,
    string tubeType,
    Guid technicianId,
    IReadOnlyCollection<Guid> examIds,
    DateTime createdAt
) : DomainEvent
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid SampleId { get; } = sampleId;
    public Guid PatientId { get; } = patientId;
    public Guid OrderId { get; } = orderId;
    public string BarcodeValue { get; } = barcodeValue;
    public string TubeType { get; } = tubeType;
    public Guid TechnicianId { get; } = technicianId;
    public IReadOnlyCollection<Guid> ExamIds { get; } = examIds;
    public DateTime CreatedAt { get; } = createdAt;
}
