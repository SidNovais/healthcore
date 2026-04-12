using HC.LIS.API.Common;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Application.SignedReports.SignReport;

namespace HC.LIS.API.Modules.LabAnalysis.WorklistItems.SignReport;

internal static class SignReportEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        SignReportRequest request,
        ILabAnalysisModule module,
        CancellationToken ct)
    {
        var reportId = await module.ExecuteCommandAsync(
            new SignReportCommand(id, request.Signature, request.SignedBy)).ConfigureAwait(false);

        return TypedResults.Created($"/api/v1/worklist-items/{id}/signed-report", new CreatedIdResponse(reportId));
    }
}
