using HC.Core.Domain;
using HC.LIS.API.Common;
using HC.LIS.Modules.SampleCollection.Application.Collections.CreateCollectionRequest;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.API.Modules.SampleCollection.CollectionRequests.CreateCollectionRequest;

internal static class CreateCollectionRequestEndpoint
{
    internal static async Task<IResult> Handle(
        CreateCollectionRequestRequest request,
        ISampleCollectionModule module,
        CancellationToken ct)
    {
        var id = await module.ExecuteCommandAsync(new CreateCollectionRequestCommand(
            request.CollectionRequestId,
            request.PatientId,
            request.ExamPreparationVerified,
            SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.Created($"/api/v1/collection-requests/{id}", new CreatedIdResponse(id));
    }
}
