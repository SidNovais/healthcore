using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.TestOrders.Domain.Physicians;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Physicians;

internal class RequestingPhysicianProvider(
    ISqlConnectionFactory sqlConnectionFactory
) : IRequestingPhysicianProvider
{
    private const string ActiveStatus = "Active";
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<RequestingPhysician?> GetByIdAsync(Guid physicianId, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT ""Id"", ""FullName"", ""Status""
            FROM ""test_orders"".""PhysicianDetails""
            WHERE ""Id"" = @PhysicianId";

        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to get the requesting physician");

        RequestingPhysicianRow? row = await connection
            .QueryFirstOrDefaultAsync<RequestingPhysicianRow>(sql, new { PhysicianId = physicianId })
            .ConfigureAwait(false);

        if (row is null)
            return null;

        return RequestingPhysician.Of(
            new PhysicianId(row.Id),
            row.FullName,
            string.Equals(row.Status, ActiveStatus, StringComparison.Ordinal));
    }

    private sealed class RequestingPhysicianRow
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
    }
}
