using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Queries;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestDetails;

internal class GetCollectionRequestDetailsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetCollectionRequestDetailsQuery, CollectionRequestDetailsDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<CollectionRequestDetailsDto?> Handle(
        GetCollectionRequestDetailsQuery query,
        CancellationToken cancellationToken
    )
    {
        string sql = @$"SELECT
            ""CollectionRequestDetails"".""Id"" AS ""{nameof(CollectionRequestDetailsDto.CollectionRequestId)}"",
            ""CollectionRequestDetails"".""PatientId"" AS ""{nameof(CollectionRequestDetailsDto.PatientId)}"",
            ""CollectionRequestDetails"".""OrderId"" AS ""{nameof(CollectionRequestDetailsDto.OrderId)}"",
            ""CollectionRequestDetails"".""Status"" AS ""{nameof(CollectionRequestDetailsDto.Status)}"",
            ""CollectionRequestDetails"".""ArrivedAt"" AS ""{nameof(CollectionRequestDetailsDto.ArrivedAt)}"",
            ""CollectionRequestDetails"".""WaitingAt"" AS ""{nameof(CollectionRequestDetailsDto.WaitingAt)}"",
            ""CollectionRequestDetails"".""CalledAt"" AS ""{nameof(CollectionRequestDetailsDto.CalledAt)}""
            FROM ""sample_collection"".""CollectionRequestDetails"" AS ""CollectionRequestDetails""
            WHERE ""CollectionRequestDetails"".""Id"" = @CollectionRequestId";

        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to get collection request details");

        return await connection.QueryFirstOrDefaultAsync<CollectionRequestDetailsDto>(
            sql,
            new { query.CollectionRequestId }
        ).ConfigureAwait(false);
    }
}
