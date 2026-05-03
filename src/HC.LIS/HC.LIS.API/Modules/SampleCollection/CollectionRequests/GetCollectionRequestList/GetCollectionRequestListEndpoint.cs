using System.Collections.Generic;
using HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestList;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.API.Modules.SampleCollection.CollectionRequests.GetCollectionRequestList;

internal static class GetCollectionRequestListEndpoint
{
    internal static async Task<IResult> Handle(
        string? status,
        int? page,
        int? perPage,
        ISampleCollectionModule module,
        CancellationToken ct)
    {
        IReadOnlyCollection<CollectionRequestSummaryDto> result = await module.ExecuteQueryAsync(
            new GetCollectionRequestListQuery(status, page, perPage)).ConfigureAwait(false);

        return TypedResults.Ok(result);
    }
}
