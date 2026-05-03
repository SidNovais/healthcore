using System.Collections.Generic;
using System.Data;
using Dapper;
using HC.Core.Application.Queries;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Queries;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestList;

internal class GetCollectionRequestListQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetCollectionRequestListQuery, IReadOnlyCollection<CollectionRequestSummaryDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<IReadOnlyCollection<CollectionRequestSummaryDto>> Handle(
        GetCollectionRequestListQuery query,
        CancellationToken cancellationToken)
    {
        const string baseSql = $"""
            SELECT
                "CollectionRequestDetails"."Id"        AS "{nameof(CollectionRequestSummaryDto.CollectionRequestId)}",
                "CollectionRequestDetails"."PatientId" AS "{nameof(CollectionRequestSummaryDto.PatientId)}",
                "CollectionRequestDetails"."Status"    AS "{nameof(CollectionRequestSummaryDto.Status)}",
                "CollectionRequestDetails"."ArrivedAt" AS "{nameof(CollectionRequestSummaryDto.ArrivedAt)}",
                "CollectionRequestDetails"."WaitingAt" AS "{nameof(CollectionRequestSummaryDto.WaitingAt)}",
                "CollectionRequestDetails"."CalledAt"  AS "{nameof(CollectionRequestSummaryDto.CalledAt)}"
            FROM "sample_collection"."CollectionRequestDetails" AS "CollectionRequestDetails"
            WHERE (@Status IS NULL OR "CollectionRequestDetails"."Status" = @Status)
            ORDER BY "CollectionRequestDetails"."ArrivedAt"
            """;

        string sql = PagedQueryHelper.AppendPageStatement(baseSql);
        PageData pageData = PagedQueryHelper.GetPageData(query);

        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Database connection is unavailable.");

        IEnumerable<CollectionRequestSummaryDto> results = await connection.QueryAsync<CollectionRequestSummaryDto>(
            sql, new { query.Status, pageData.Offset, pageData.Next }
        ).ConfigureAwait(false);

        return results.AsList().AsReadOnly();
    }
}
