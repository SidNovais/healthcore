using HC.LIS.API.Common;
using HC.LIS.API.Modules.SampleCollection.CollectionRequests.CallPatient;
using HC.LIS.API.Modules.SampleCollection.CollectionRequests.CreateBarcode;
using HC.LIS.API.Modules.SampleCollection.CollectionRequests.CreateCollectionRequest;
using HC.LIS.API.Modules.SampleCollection.CollectionRequests.GetCollectionRequestDetails;
using HC.LIS.API.Modules.SampleCollection.CollectionRequests.MovePatientToWaiting;
using HC.LIS.API.Modules.SampleCollection.CollectionRequests.RecordSampleCollection;
using HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestDetails;

namespace HC.LIS.API.Modules.SampleCollection.CollectionRequests;

internal static class CollectionRequestsEndpoints
{
    internal static RouteGroupBuilder MapCollectionRequestsEndpoints(
        this RouteGroupBuilder group)
    {
        group.WithTags("CollectionRequests");

        group.MapPost("", CreateCollectionRequestEndpoint.Handle)
            .WithName("CreateCollectionRequest")
            .WithSummary("Create a new collection request.")
            .Produces<CreatedIdResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("{id:guid}", GetCollectionRequestDetailsEndpoint.Handle)
            .WithName("GetCollectionRequestDetails")
            .WithSummary("Get collection request details by ID.")
            .Produces<CollectionRequestDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("{id:guid}/move-to-waiting", MovePatientToWaitingEndpoint.Handle)
            .WithName("MovePatientToWaiting")
            .WithSummary("Move the patient to the waiting state.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("{id:guid}/call-patient", CallPatientEndpoint.Handle)
            .WithName("CallPatient")
            .WithSummary("Call the patient to the collection station.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("{id:guid}/barcodes", CreateBarcodeEndpoint.Handle)
            .WithName("CreateBarcode")
            .WithSummary("Create a barcode for a collection tube.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("{id:guid}/collect", RecordSampleCollectionEndpoint.Handle)
            .WithName("RecordSampleCollection")
            .WithSummary("Record the sample collection.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }
}
