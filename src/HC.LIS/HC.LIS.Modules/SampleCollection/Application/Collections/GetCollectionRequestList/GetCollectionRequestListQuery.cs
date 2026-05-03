using System.Collections.Generic;
using HC.Core.Application.Queries;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestList;

public class GetCollectionRequestListQuery(
    string? status = null,
    int? page = null,
    int? perPage = null
) : QueryBase<IReadOnlyCollection<CollectionRequestSummaryDto>>, IPagedQuery
{
    public string? Status { get; } = status;
    public int? Page { get; } = page;
    public int? PerPage { get; } = perPage;
}
