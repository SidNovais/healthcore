using System.Collections.Generic;
using HC.LIS.Modules.SampleCollection.Application.Collections.GetSamplesByCollectionRequestId;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.API.Modules.SampleCollection.CollectionRequests.GetSamplesByCollectionRequestId;

internal static class GetSamplesByCollectionRequestIdEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        ISampleCollectionModule module,
        CancellationToken ct)
    {
        IReadOnlyCollection<SampleSummaryDto> result = await module.ExecuteQueryAsync(
            new GetSamplesByCollectionRequestIdQuery(id)).ConfigureAwait(false);

        return TypedResults.Ok(result);
    }
}
