using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Queries;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.GetSamplesByCollectionRequestId;

internal class GetSamplesByCollectionRequestIdQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetSamplesByCollectionRequestIdQuery, IReadOnlyCollection<SampleSummaryDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<IReadOnlyCollection<SampleSummaryDto>> Handle(
        GetSamplesByCollectionRequestIdQuery query,
        CancellationToken cancellationToken)
    {
        string sql = @$"SELECT
            ""SampleDetails"".""Id""       AS ""{nameof(SampleSummaryDto.Id)}"",
            ""SampleDetails"".""TubeType"" AS ""{nameof(SampleSummaryDto.TubeType)}"",
            ""SampleDetails"".""Barcode""  AS ""{nameof(SampleSummaryDto.Barcode)}"",
            ""SampleDetails"".""Status""   AS ""{nameof(SampleSummaryDto.Status)}""
            FROM ""sample_collection"".""SampleDetails""
            WHERE ""SampleDetails"".""CollectionRequestId"" = @CollectionRequestId";

        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("No connection available");

        var results = await connection
            .QueryAsync<SampleSummaryDto>(sql, new { query.CollectionRequestId })
            .ConfigureAwait(false);

        return results.AsList().AsReadOnly();
    }
}
