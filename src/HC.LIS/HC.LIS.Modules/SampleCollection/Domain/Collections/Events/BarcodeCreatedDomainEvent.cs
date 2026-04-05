using System;
using System.Collections.Generic;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

public class BarcodeCreatedDomainEvent(
    Guid collectionRequestId,
    Guid sampleId,
    Guid patientId,
    string barcodeValue,
    string tubeType,
    Guid technicianId,
    IReadOnlyCollection<CollectionExam> exams,
    DateTime createdAt
) : DomainEvent
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid SampleId { get; } = sampleId;
    public Guid PatientId { get; } = patientId;
    public string BarcodeValue { get; } = barcodeValue;
    public string TubeType { get; } = tubeType;
    public Guid TechnicianId { get; } = technicianId;
    public IReadOnlyCollection<CollectionExam> Exams { get; } = exams;
    public DateTime CreatedAt { get; } = createdAt;
}
