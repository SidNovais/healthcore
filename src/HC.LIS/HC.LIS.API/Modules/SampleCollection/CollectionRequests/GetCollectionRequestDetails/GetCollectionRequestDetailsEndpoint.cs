using HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestDetails;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.API.Modules.SampleCollection.CollectionRequests.GetCollectionRequestDetails;

internal static class GetCollectionRequestDetailsEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        ISampleCollectionModule module,
        CancellationToken ct)
    {
        var result = await module.ExecuteQueryAsync(
            new GetCollectionRequestDetailsQuery(id)).ConfigureAwait(false);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}
