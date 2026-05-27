using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.PatientManagement.Application.Configuration.Queries;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.SearchPatients;

internal class SearchPatientsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<SearchPatientsQuery, IReadOnlyCollection<PatientSearchResultDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<IReadOnlyCollection<PatientSearchResultDto>> Handle(
        SearchPatientsQuery query,
        CancellationToken cancellationToken
    )
    {
        const string sql = @"
            SELECT
                ""Id"",
                ""FullName"",
                ""DateOfBirth"",
                ""DocumentId"",
                ""Status""
            FROM patient_management.""PatientDetails""
            WHERE ""FullName"" ILIKE @SearchTerm OR ""DocumentId"" ILIKE @SearchTerm
            ORDER BY ""FullName""";

        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to search patients");

        IEnumerable<PatientSearchResultDto> results = await connection
            .QueryAsync<PatientSearchResultDto>(sql, new { SearchTerm = $"%{query.SearchTerm}%" })
            .ConfigureAwait(false);

        return results.ToList().AsReadOnly();
    }
}
