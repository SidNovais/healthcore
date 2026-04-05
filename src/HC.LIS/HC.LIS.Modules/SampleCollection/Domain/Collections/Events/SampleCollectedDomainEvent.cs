using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

public class SampleCollectedDomainEvent(
    Guid collectionRequestId,
    Guid sampleId,
    Guid patientId,
    Guid technicianId,
    IReadOnlyCollection<CollectionExam> exams,
    string sampleBarcode,
    DateTime collectedAt
) : DomainEvent
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid SampleId { get; } = sampleId;
    public Guid PatientId { get; } = patientId;
    public Guid TechnicianId { get; } = technicianId;
    public IReadOnlyCollection<CollectionExam> Exams { get; } = exams;
    public string SampleBarcode { get; } = sampleBarcode;
    public DateTime CollectedAt { get; } = collectedAt;
}
