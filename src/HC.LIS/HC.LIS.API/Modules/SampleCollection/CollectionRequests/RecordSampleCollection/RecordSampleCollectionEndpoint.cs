using HC.Core.Application;
using HC.Core.Domain;
using HC.LIS.Modules.SampleCollection.Application.Collections.RecordSampleCollection;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.API.Modules.SampleCollection.CollectionRequests.RecordSampleCollection;

internal static class RecordSampleCollectionEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        RecordSampleCollectionRequest request,
        ISampleCollectionModule module,
        IExecutionContextAccessor executionContext,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(new RecordSampleCollectionCommand(
            id,
            request.SampleId,
            executionContext.UserId,
            SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
