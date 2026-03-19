using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

public class PatientCalledDomainEvent(
    Guid collectionRequestId,
    Guid patientId,
    Guid technicianId,
    DateTime calledAt
) : DomainEvent
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid PatientId { get; } = patientId;
    public Guid TechnicianId { get; } = technicianId;
    public DateTime CalledAt { get; } = calledAt;
}
