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
        string sql = @$"SELECT
            ""PhysicianDetails"".""Id"" AS ""{nameof(PhysicianDetailsDto.Id)}"",
            ""PhysicianDetails"".""FullName"" AS ""{nameof(PhysicianDetailsDto.FullName)}"",
            ""PhysicianDetails"".""LicenceNumber"" AS ""{nameof(PhysicianDetailsDto.LicenceNumber)}"",
            ""PhysicianDetails"".""Status"" AS ""{nameof(PhysicianDetailsDto.Status)}"",
            ""PhysicianDetails"".""RegisteredAt"" AS ""{nameof(PhysicianDetailsDto.RegisteredAt)}"",
            ""PhysicianDetails"".""UpdatedAt"" AS ""{nameof(PhysicianDetailsDto.UpdatedAt)}"",
            ""PhysicianDetails"".""DeactivatedAt"" AS ""{nameof(PhysicianDetailsDto.DeactivatedAt)}""
            FROM ""test_orders"".""PhysicianDetails"" AS ""PhysicianDetails""
            WHERE ""PhysicianDetails"".""Id"" = @PhysicianId";

        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to get physician details");

        return await connection.QueryFirstOrDefaultAsync<PhysicianDetailsDto>(
            sql,
            new
            {
                query.PhysicianId
            }
        ).ConfigureAwait(false);
    }
}
