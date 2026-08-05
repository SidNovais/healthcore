using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.LabAnalysis.Application.Orders;
using HC.LIS.Modules.LabAnalysis.Application.Physicians;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Orders;

internal class OrderPhysicianRepository(ISqlConnectionFactory sqlConnectionFactory) : IOrderPhysicianRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<PhysicianSnapshotView?> GetRequestingPhysicianAsync(Guid orderId)
    {
        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to read the requesting physician");
        const string sql = """
            SELECT psd."FullName", psd."LicenceNumber"
            FROM "lab_analysis"."OrderPhysicianSnapshotDetails" AS opsd
            INNER JOIN "lab_analysis"."PhysicianSnapshotDetails" AS psd ON psd."Id" = opsd."PhysicianId"
            WHERE opsd."OrderId" = @OrderId
            """;
        return await connection.QuerySingleOrDefaultAsync<PhysicianSnapshotView>(
            sql, new { OrderId = orderId }).ConfigureAwait(false);
    }

    public async Task StoreAsync(Guid orderId, Guid physicianId, DateTime requestedAt)
    {
        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to store the order physician");
        const string sql = """
            INSERT INTO "lab_analysis"."OrderPhysicianSnapshotDetails"
                ("OrderId", "PhysicianId", "RequestedAt")
            VALUES
                (@OrderId, @PhysicianId, @RequestedAt)
            ON CONFLICT ("OrderId") DO NOTHING
            """;
        await connection.ExecuteAsync(sql, new
        {
            OrderId = orderId,
            PhysicianId = physicianId,
            RequestedAt = requestedAt
        }).ConfigureAwait(false);
    }
}
