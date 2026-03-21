using System;
using HC.COre.Infrastructure.EventBus;

namespace HC.LIS.Modules.SampleCollection.IntegrationEvents;

public class BarcodeCreatedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid collectionRequestId,
    Guid sampleId,
    string barcode
) : IntegrationEvent(id, occurredAt)
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid SampleId { get; } = sampleId;
    public string Barcode { get; } = barcode;
}
