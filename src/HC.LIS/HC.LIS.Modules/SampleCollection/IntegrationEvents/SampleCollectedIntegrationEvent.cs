using System;
using HC.COre.Infrastructure.EventBus;

namespace HC.LIS.Modules.SampleCollection.IntegrationEvents;

public class SampleCollectedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid collectionRequestId,
    Guid sampleId,
    Guid patientId,
    string sampleBarcode,
    IReadOnlyCollection<string> examCodes
) : IntegrationEvent(id, occurredAt)
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid SampleId { get; } = sampleId;
    public Guid PatientId { get; } = patientId;
    public string SampleBarcode { get; } = sampleBarcode;
    public IReadOnlyCollection<string> ExamCodes { get; } = examCodes;
}
