using HC.LIS.Modules.SampleCollection.Application.Collections.GetSampleDetails;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.API.Modules.SampleCollection.Samples.GetSampleDetails;

internal static class GetSampleDetailsEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        ISampleCollectionModule module,
        CancellationToken ct)
    {
        var result = await module.ExecuteQueryAsync(
            new GetSampleDetailsQuery(id)).ConfigureAwait(false);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}
