using HC.LIS.API.Common;
using HC.LIS.API.Modules.LabAnalysis.WorklistItems.GetSignedReport;
using HC.LIS.API.Modules.LabAnalysis.WorklistItems.GetWorklistItemDetails;
using HC.LIS.API.Modules.LabAnalysis.WorklistItems.SignReport;
using HC.LIS.Modules.LabAnalysis.Application.SignedReports.GetSignedReportDetails;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;

namespace HC.LIS.API.Modules.LabAnalysis.WorklistItems;

internal static class WorklistItemsEndpoints
{
    internal static RouteGroupBuilder MapWorklistItemsEndpoints(
        this RouteGroupBuilder group)
    {
        group.WithTags("WorklistItems");

        group.MapGet("{id:guid}", GetWorklistItemDetailsEndpoint.Handle)
            .WithName("GetWorklistItemDetails")
            .WithSummary("Get worklist item details by ID.")
            .Produces<WorklistItemDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("{id:guid}/signed-report", GetSignedReportEndpoint.Handle)
            .WithName("GetSignedReport")
            .WithSummary("Get the signed report for a worklist item.")
            .Produces<SignedReportDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("{id:guid}/sign", SignReportEndpoint.Handle)
            .WithName("SignReport")
            .WithSummary("Sign the report for a worklist item.")
            .Produces<CreatedIdResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }
}
