using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.RecordSampleCollection;

public class RecordSampleCollectionCommand(
    Guid collectionRequestId,
    Guid sampleId,
    Guid technicianId,
    DateTime collectedAt
) : CommandBase
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid SampleId { get; } = sampleId;
    public Guid TechnicianId { get; } = technicianId;
    public DateTime CollectedAt { get; } = collectedAt;
}
