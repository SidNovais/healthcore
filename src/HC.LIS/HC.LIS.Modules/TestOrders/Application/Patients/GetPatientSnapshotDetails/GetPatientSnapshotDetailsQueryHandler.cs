using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.TestOrders.Application.Configuration.Queries;

namespace HC.LIS.Modules.TestOrders.Application.Patients.GetPatientSnapshotDetails;

internal class GetPatientSnapshotDetailsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetPatientSnapshotDetailsQuery, PatientSnapshotDetailsDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<PatientSnapshotDetailsDto?> Handle(
        GetPatientSnapshotDetailsQuery query,
        CancellationToken cancellationToken
    )
    {
        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to get patient snapshot details");
        const string sql = """
            SELECT
                "Id" AS "PatientId",
                "FullName",
                "DateOfBirth",
                "Gender",
                "MothersFullName",
                "DocumentId",
                "Phone",
                "Email",
                "Status",
                "RegisteredAt",
                "AnonymizedAt"
            FROM test_orders."PatientSnapshotDetails"
            WHERE "Id" = @PatientId
            """;
        return await connection
            .QueryFirstOrDefaultAsync<PatientSnapshotDetailsDto>(sql, new { query.PatientId })
            .ConfigureAwait(false);
    }
}
