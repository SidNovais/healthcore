using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.LabAnalysis.Application.Physicians;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Physicians;

internal class PhysicianSnapshotRepository(ISqlConnectionFactory sqlConnectionFactory) : IPhysicianSnapshotRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<PhysicianSnapshotView?> GetByIdAsync(Guid physicianId)
    {
        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to read physician snapshot");
        const string sql = """
            SELECT "FullName", "LicenceNumber"
            FROM "lab_analysis"."PhysicianSnapshotDetails"
            WHERE "Id" = @PhysicianId
            """;
        return await connection.QuerySingleOrDefaultAsync<PhysicianSnapshotView>(
            sql, new { PhysicianId = physicianId }).ConfigureAwait(false);
    }

    public async Task StoreAsync(Guid physicianId, string fullName, string? licenceNumber, DateTime registeredAt)
    {
        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to store physician snapshot");
        const string sql = """
            INSERT INTO "lab_analysis"."PhysicianSnapshotDetails"
                ("Id", "FullName", "LicenceNumber", "Status", "RegisteredAt")
            VALUES
                (@PhysicianId, @FullName, @LicenceNumber, 'Active', @RegisteredAt)
            ON CONFLICT ("Id") DO NOTHING
            """;
        await connection.ExecuteAsync(sql, new
        {
            PhysicianId = physicianId,
            FullName = fullName,
            LicenceNumber = licenceNumber,
            RegisteredAt = registeredAt
        }).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Guid physicianId, string fullName, string? licenceNumber, DateTime updatedAt)
    {
        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to update physician snapshot");
        const string sql = """
            UPDATE "lab_analysis"."PhysicianSnapshotDetails"
            SET "FullName" = @FullName,
                "LicenceNumber" = @LicenceNumber,
                "UpdatedAt" = @UpdatedAt
            WHERE "Id" = @PhysicianId
            """;
        await connection.ExecuteAsync(sql, new
        {
            PhysicianId = physicianId,
            FullName = fullName,
            LicenceNumber = licenceNumber,
            UpdatedAt = updatedAt
        }).ConfigureAwait(false);
    }

    public async Task DeactivateAsync(Guid physicianId, DateTime deactivatedAt)
    {
        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to deactivate physician snapshot");
        const string sql = """
            UPDATE "lab_analysis"."PhysicianSnapshotDetails"
            SET "Status" = 'Inactive',
                "DeactivatedAt" = @DeactivatedAt
            WHERE "Id" = @PhysicianId
            """;
        await connection.ExecuteAsync(sql, new
        {
            PhysicianId = physicianId,
            DeactivatedAt = deactivatedAt
        }).ConfigureAwait(false);
    }

    public async Task ReactivateAsync(Guid physicianId, DateTime reactivatedAt)
    {
        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to reactivate physician snapshot");
        const string sql = """
            UPDATE "lab_analysis"."PhysicianSnapshotDetails"
            SET "Status" = 'Active',
                "DeactivatedAt" = NULL,
                "UpdatedAt" = @ReactivatedAt
            WHERE "Id" = @PhysicianId
            """;
        await connection.ExecuteAsync(sql, new
        {
            PhysicianId = physicianId,
            ReactivatedAt = reactivatedAt
        }).ConfigureAwait(false);
    }
}
