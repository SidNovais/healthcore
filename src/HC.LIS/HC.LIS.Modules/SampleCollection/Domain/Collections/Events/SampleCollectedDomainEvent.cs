using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

public class SampleCollectedDomainEvent(
    Guid collectionRequestId,
    Guid sampleId,
    Guid patientId,
    Guid technicianId,
    IReadOnlyCollection<Guid> examIds,
    DateTime collectedAt
) : DomainEvent
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid SampleId { get; } = sampleId;
    public Guid PatientId { get; } = patientId;
    public Guid TechnicianId { get; } = technicianId;
    public IReadOnlyCollection<Guid> ExamIds { get; } = examIds;
    public DateTime CollectedAt { get; } = collectedAt;
}
