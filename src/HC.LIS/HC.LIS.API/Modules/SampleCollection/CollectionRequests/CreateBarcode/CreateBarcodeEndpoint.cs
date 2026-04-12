using HC.Core.Domain;
using HC.LIS.Modules.SampleCollection.Application.Collections.CreateBarcode;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.API.Modules.SampleCollection.CollectionRequests.CreateBarcode;

internal static class CreateBarcodeEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        CreateBarcodeRequest request,
        ISampleCollectionModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(new CreateBarcodeCommand(
            id,
            request.TubeType,
            request.BarcodeValue,
            request.TechnicianId,
            SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
