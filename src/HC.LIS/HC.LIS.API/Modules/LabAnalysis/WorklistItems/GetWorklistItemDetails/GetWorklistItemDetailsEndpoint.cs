using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;

namespace HC.LIS.API.Modules.LabAnalysis.WorklistItems.GetWorklistItemDetails;

internal static class GetWorklistItemDetailsEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        ILabAnalysisModule module,
        CancellationToken ct)
    {
        var result = await module.ExecuteQueryAsync(
            new GetWorklistItemDetailsQuery(id)).ConfigureAwait(false);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}
