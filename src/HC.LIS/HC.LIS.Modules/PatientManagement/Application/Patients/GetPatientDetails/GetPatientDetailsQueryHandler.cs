using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.PatientManagement.Application.Configuration.Queries;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.GetPatientDetails;

internal class GetPatientDetailsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetPatientDetailsQuery, PatientDetailsDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<PatientDetailsDto?> Handle(
        GetPatientDetailsQuery query,
        CancellationToken cancellationToken
    )
    {
        const string sql = @"
            SELECT
                ""Id"",
                ""FullName"",
                ""DateOfBirth"",
                ""Gender"",
                ""MothersFullName"",
                ""DocumentId"",
                ""Phone"",
                ""Email"",
                ""Status"",
                ""RegisteredAt"",
                ""AnonymizedAt""
            FROM patient_management.""PatientDetails""
            WHERE ""Id"" = @PatientId";

        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to get patient details");

        return await connection
            .QueryFirstOrDefaultAsync<PatientDetailsDto>(sql, new { query.PatientId })
            .ConfigureAwait(false);
    }
}
