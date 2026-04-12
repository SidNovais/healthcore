using HC.Core.Domain;
using HC.LIS.Modules.SampleCollection.Application.Collections.MovePatientToWaiting;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.API.Modules.SampleCollection.CollectionRequests.MovePatientToWaiting;

internal static class MovePatientToWaitingEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        ISampleCollectionModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(
            new MovePatientToWaitingCommand(id, SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
