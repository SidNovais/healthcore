using System.Collections.Generic;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.GetSamplesByCollectionRequestId;

public class GetSamplesByCollectionRequestIdQuery(Guid collectionRequestId)
    : QueryBase<IReadOnlyCollection<SampleSummaryDto>>
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
}
