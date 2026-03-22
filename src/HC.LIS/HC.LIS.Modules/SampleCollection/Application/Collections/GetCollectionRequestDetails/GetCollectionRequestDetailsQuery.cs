using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestDetails;

public class GetCollectionRequestDetailsQuery(
    Guid collectionRequestId
) : QueryBase<CollectionRequestDetailsDto?>
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
}
