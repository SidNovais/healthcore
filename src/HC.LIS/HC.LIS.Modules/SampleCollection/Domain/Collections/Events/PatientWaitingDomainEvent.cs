using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

public class PatientWaitingDomainEvent(
    Guid collectionRequestId,
    Guid patientId,
    DateTime waitingAt
) : DomainEvent
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid PatientId { get; } = patientId;
    public DateTime WaitingAt { get; } = waitingAt;
}
