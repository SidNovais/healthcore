using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

public class AllSamplesCollectedDomainEvent(
    Guid collectionRequestId,
    DateTime collectedAt
) : DomainEvent
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public DateTime CollectedAt { get; } = collectedAt;
}
