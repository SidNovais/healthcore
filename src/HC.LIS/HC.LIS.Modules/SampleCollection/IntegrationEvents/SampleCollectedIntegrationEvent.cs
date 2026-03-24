using System;
using HC.COre.Infrastructure.EventBus;

namespace HC.LIS.Modules.SampleCollection.IntegrationEvents;

public class SampleCollectedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid collectionRequestId,
    Guid sampleId,
    IReadOnlyCollection<Guid> examIds
) : IntegrationEvent(id, occurredAt)
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid SampleId { get; } = sampleId;
    public IReadOnlyCollection<Guid> ExamIds { get; } = examIds;
}
