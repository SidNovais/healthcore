namespace HC.Core.Application.Queries;

public static class PagedQueryHelper
{
    public const string Offset = "Offset";
    public const string Next = "Next";

    public static PageData GetPageData(IPagedQuery query)
    {
        int offset = query.Page.HasValue && query.PerPage.HasValue
            ? (query.Page.Value - 1) * query.PerPage.Value
            : 0;

        int next = query.PerPage ?? int.MaxValue;

        return new PageData(offset, next);
    }

    public static string AppendPageStatement(string sql) =>
        $"{sql} OFFSET @{Offset} LIMIT @{Next}";
}
