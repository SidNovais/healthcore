using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Queries;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.GetSampleDetails;

internal class GetSampleDetailsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetSampleDetailsQuery, SampleDetailsDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<SampleDetailsDto?> Handle(
        GetSampleDetailsQuery query,
        CancellationToken cancellationToken
    )
    {
        string sql = @$"SELECT
            ""SampleDetails"".""Id"" AS ""{nameof(SampleDetailsDto.SampleId)}"",
            ""SampleDetails"".""CollectionRequestId"" AS ""{nameof(SampleDetailsDto.CollectionRequestId)}"",
            ""SampleDetails"".""TubeType"" AS ""{nameof(SampleDetailsDto.TubeType)}"",
            ""SampleDetails"".""Barcode"" AS ""{nameof(SampleDetailsDto.Barcode)}"",
            ""SampleDetails"".""Status"" AS ""{nameof(SampleDetailsDto.Status)}"",
            ""SampleDetails"".""CollectedAt"" AS ""{nameof(SampleDetailsDto.CollectedAt)}""
            FROM ""sample_collection"".""SampleDetails"" AS ""SampleDetails""
            WHERE ""SampleDetails"".""Id"" = @SampleId";

        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to get sample details");

        return await connection.QueryFirstOrDefaultAsync<SampleDetailsDto>(
            sql,
            new { query.SampleId }
        ).ConfigureAwait(false);
    }
}
