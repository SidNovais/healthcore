using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.TestOrders.Application.Configuration.Queries;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.GetPhysicianDetails;

internal class GetPhysicianDetailsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetPhysicianDetailsQuery, PhysicianDetailsDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<PhysicianDetailsDto?> Handle(
        GetPhysicianDetailsQuery query,
        CancellationToken cancellationToken
    )
    {
        const string sql = @"
            SELECT
                ""Id"",
                ""FullName"",
                ""LicenceNumber"",
                ""Status"",
                ""RegisteredAt"",
                ""UpdatedAt"",
                ""DeactivatedAt""
            FROM test_orders.""PhysicianDetails""
            WHERE ""Id"" = @PhysicianId";

        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to get physician details");

        return await connection
            .QueryFirstOrDefaultAsync<PhysicianDetailsDto>(sql, new { query.PhysicianId })
            .ConfigureAwait(false);
    }
}
