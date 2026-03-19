using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

public class SampleCollectedDomainEvent(
    Guid collectionRequestId,
    Guid sampleId,
    Guid patientId,
    Guid technicianId,
    DateTime collectedAt
) : DomainEvent
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid SampleId { get; } = sampleId;
    public Guid PatientId { get; } = patientId;
    public Guid TechnicianId { get; } = technicianId;
    public DateTime CollectedAt { get; } = collectedAt;
}
