using HC.Core.Domain;
using HC.LIS.Modules.SampleCollection.Application.Collections.CallPatient;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.API.Modules.SampleCollection.CollectionRequests.CallPatient;

internal static class CallPatientEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        CallPatientRequest request,
        ISampleCollectionModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(
            new CallPatientCommand(id, request.TechnicianId, SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
