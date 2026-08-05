using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.TestOrders.Application.Configuration.Queries;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.SearchPhysicians;

internal class SearchPhysiciansQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<SearchPhysiciansQuery, IReadOnlyCollection<PhysicianSearchResultDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<IReadOnlyCollection<PhysicianSearchResultDto>> Handle(
        SearchPhysiciansQuery query,
        CancellationToken cancellationToken
    )
    {
        string sql = @$"SELECT
            ""PhysicianDetails"".""Id"" AS ""{nameof(PhysicianSearchResultDto.Id)}"",
            ""PhysicianDetails"".""FullName"" AS ""{nameof(PhysicianSearchResultDto.FullName)}"",
            ""PhysicianDetails"".""LicenceNumber"" AS ""{nameof(PhysicianSearchResultDto.LicenceNumber)}"",
            ""PhysicianDetails"".""Status"" AS ""{nameof(PhysicianSearchResultDto.Status)}""
            FROM ""test_orders"".""PhysicianDetails"" AS ""PhysicianDetails""
            WHERE (""PhysicianDetails"".""FullName"" ILIKE @SearchTerm
                OR ""PhysicianDetails"".""LicenceNumber"" ILIKE @SearchTerm)
              AND (@IncludeInactive OR ""PhysicianDetails"".""Status"" = 'Active')
            ORDER BY ""PhysicianDetails"".""FullName""";

        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to search physicians");

        const string MatchEveryPhysician = "%";
        string prefixPattern = string.IsNullOrWhiteSpace(query.SearchTerm)
            ? MatchEveryPhysician
            : $"{query.SearchTerm}%";

        IEnumerable<PhysicianSearchResultDto> results = await connection
            .QueryAsync<PhysicianSearchResultDto>(
                sql,
                new
                {
                    SearchTerm = prefixPattern,
                    query.IncludeInactive
                }
            ).ConfigureAwait(false);

        return results.ToList().AsReadOnly();
    }
}
