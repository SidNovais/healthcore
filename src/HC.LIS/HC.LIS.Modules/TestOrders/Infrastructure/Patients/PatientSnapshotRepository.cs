using System.Data;
using System.Threading.Tasks;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.TestOrders.Application.Patients;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Patients;

internal class PatientSnapshotRepository(ISqlConnectionFactory sqlConnectionFactory) : IPatientSnapshotRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task StoreAsync(
        Guid patientId,
        string fullName,
        DateTime dateOfBirth,
        string? gender,
        string? mothersFullName,
        string? documentId,
        string? phone,
        string? email,
        DateTime registeredAt)
    {
        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to store patient snapshot");
        const string sql = """
            INSERT INTO "test_orders"."PatientSnapshotDetails"
                ("Id", "FullName", "DateOfBirth", "Gender", "MothersFullName", "DocumentId", "Phone", "Email", "Status", "RegisteredAt")
            VALUES
                (@PatientId, @FullName, @DateOfBirth, @Gender, @MothersFullName, @DocumentId, @Phone, @Email, 'Active', @RegisteredAt)
            """;
        await connection.ExecuteAsync(sql, new
        {
            PatientId = patientId,
            FullName = fullName,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            MothersFullName = mothersFullName,
            DocumentId = documentId,
            Phone = phone,
            Email = email,
            RegisteredAt = registeredAt
        }).ConfigureAwait(false);
    }

    public async Task UpdateAsync(
        Guid patientId,
        string fullName,
        DateTime dateOfBirth,
        string? gender,
        string? mothersFullName,
        string? documentId,
        string? phone,
        string? email)
    {
        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to update patient snapshot");
        const string sql = """
            UPDATE "test_orders"."PatientSnapshotDetails"
            SET "FullName" = @FullName,
                "DateOfBirth" = @DateOfBirth,
                "Gender" = @Gender,
                "MothersFullName" = @MothersFullName,
                "DocumentId" = @DocumentId,
                "Phone" = @Phone,
                "Email" = @Email
            WHERE "Id" = @PatientId
            """;
        await connection.ExecuteAsync(sql, new
        {
            PatientId = patientId,
            FullName = fullName,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            MothersFullName = mothersFullName,
            DocumentId = documentId,
            Phone = phone,
            Email = email
        }).ConfigureAwait(false);
    }

    public async Task AnonymizeAsync(Guid patientId, DateTime anonymizedAt)
    {
        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to anonymize patient snapshot");
        const string sql = """
            UPDATE "test_orders"."PatientSnapshotDetails"
            SET "Status" = 'Anonymized',
                "AnonymizedAt" = @AnonymizedAt
            WHERE "Id" = @PatientId
            """;
        await connection.ExecuteAsync(sql, new
        {
            PatientId = patientId,
            AnonymizedAt = anonymizedAt
        }).ConfigureAwait(false);
    }
}
