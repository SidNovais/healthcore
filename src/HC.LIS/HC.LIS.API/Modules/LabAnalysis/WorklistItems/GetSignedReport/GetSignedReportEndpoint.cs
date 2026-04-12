using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Application.SignedReports.GetSignedReportDetails;

namespace HC.LIS.API.Modules.LabAnalysis.WorklistItems.GetSignedReport;

internal static class GetSignedReportEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        ILabAnalysisModule module,
        CancellationToken ct)
    {
        var result = await module.ExecuteQueryAsync(
            new GetSignedReportByWorklistItemIdQuery(id)).ConfigureAwait(false);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}
