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
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(new RecordSampleCollectionCommand(
            id,
            request.SampleId,
            request.TechnicianId,
            request.PatientName,
            request.PatientBirthdate,
            request.PatientGender,
            SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
