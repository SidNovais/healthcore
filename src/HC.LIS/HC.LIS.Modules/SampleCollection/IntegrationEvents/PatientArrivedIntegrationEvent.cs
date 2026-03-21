using System;
using HC.COre.Infrastructure.EventBus;

namespace HC.LIS.Modules.SampleCollection.IntegrationEvents;

public class PatientArrivedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid collectionRequestId,
    Guid patientId
) : IntegrationEvent(id, occurredAt)
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid PatientId { get; } = patientId;
}
