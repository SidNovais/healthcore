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
        const string sql = @"
            SELECT
                ""Id"",
                ""FullName"",
                ""LicenceNumber"",
                ""Status""
            FROM test_orders.""PhysicianDetails""
            WHERE (""FullName"" ILIKE @SearchTerm OR ""LicenceNumber"" ILIKE @SearchTerm)
              AND (@IncludeInactive OR ""Status"" = 'Active')
            ORDER BY ""FullName""";

        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to search physicians");

        IEnumerable<PhysicianSearchResultDto> results = await connection
            .QueryAsync<PhysicianSearchResultDto>(sql, new
            {
                SearchTerm = string.IsNullOrWhiteSpace(query.SearchTerm) ? "%" : $"%{query.SearchTerm}%",
                query.IncludeInactive
            })
            .ConfigureAwait(false);

        return results.ToList().AsReadOnly();
    }
}
