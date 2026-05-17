using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.GenerateSampleBarcodes;

public class GenerateSampleBarcodesForCollectionRequestCommand(
    Guid collectionRequestId
) : CommandBase
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
}
