using System.Collections.Generic;
using HC.Core.Application.Queries;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemList;

public class GetWorklistItemListQuery(
    string? status = null,
    int? page = null,
    int? perPage = null
) : QueryBase<IReadOnlyCollection<WorklistItemSummaryDto>>, IPagedQuery
{
    public string? Status { get; } = status;
    public int? Page { get; } = page;
    public int? PerPage { get; } = perPage;
}
