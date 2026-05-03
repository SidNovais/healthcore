using System.Collections.Generic;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemList;

namespace HC.LIS.API.Modules.LabAnalysis.WorklistItems.GetWorklistItemList;

internal static class GetWorklistItemListEndpoint
{
    internal static async Task<IResult> Handle(
        string? status,
        int? page,
        int? perPage,
        ILabAnalysisModule module,
        CancellationToken ct)
    {
        IReadOnlyCollection<WorklistItemSummaryDto> result = await module.ExecuteQueryAsync(
            new GetWorklistItemListQuery(status, page, perPage)).ConfigureAwait(false);

        return TypedResults.Ok(result);
    }
}
