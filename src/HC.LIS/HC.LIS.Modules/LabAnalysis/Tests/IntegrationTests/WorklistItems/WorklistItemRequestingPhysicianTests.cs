using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using Npgsql;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemList;

namespace HC.LIS.Modules.LabAnalysis.IntegrationTests.WorklistItems;

public class WorklistItemRequestingPhysicianTests : TestBase
{
    private static readonly Guid WorklistItemId = Guid.Parse("019b664c-0000-7f37-a794-000000000020");
    private static readonly Guid PatientId      = Guid.Parse("019b664c-0000-7f37-a794-000000000021");
    private static readonly Guid OrderId        = Guid.Parse("019b664c-0000-7f37-a794-000000000022");
    private static readonly Guid PhysicianId    = Guid.Parse("019b664c-0000-7f37-a794-000000000023");

    public WorklistItemRequestingPhysicianTests() : base(Guid.CreateVersion7()) { }

    private static async Task InsertWorklistItemAsync(NpgsqlConnection connection, string sampleBarcode)
    {
        await connection.ExecuteAsync(
            """
            INSERT INTO lab_analysis.worklist_item_details
                (id, sample_id, sample_barcode, exam_code, patient_id, order_id, order_item_id, status, created_at)
            VALUES
                (@Id, @SampleId, @SampleBarcode, @ExamCode, @PatientId, @OrderId, @OrderItemId, @Status, @CreatedAt)
            """,
            new
            {
                Id            = WorklistItemId,
                SampleId      = Guid.NewGuid(),
                SampleBarcode = sampleBarcode,
                ExamCode      = "GLU",
                PatientId,
                OrderId,
                OrderItemId   = Guid.NewGuid(),
                Status        = "Pending",
                CreatedAt     = DateTime.UtcNow
            }
        ).ConfigureAwait(true);
    }

    private static async Task InsertPhysicianMappingAsync(NpgsqlConnection connection, string fullName)
    {
        await connection.ExecuteAsync(
            """
            INSERT INTO "lab_analysis"."PhysicianSnapshotDetails"
                ("Id", "FullName", "LicenceNumber", "Status", "RegisteredAt")
            VALUES
                (@Id, @FullName, @LicenceNumber, 'Active', @RegisteredAt)
            """,
            new
            {
                Id            = PhysicianId,
                FullName      = fullName,
                LicenceNumber = "CRM-12345",
                RegisteredAt  = DateTime.UtcNow
            }
        ).ConfigureAwait(true);

        await connection.ExecuteAsync(
            """
            INSERT INTO "lab_analysis"."OrderPhysicianSnapshotDetails"
                ("OrderId", "PhysicianId", "RequestedAt")
            VALUES
                (@OrderId, @PhysicianId, @RequestedAt)
            """,
            new { OrderId, PhysicianId, RequestedAt = DateTime.UtcNow }
        ).ConfigureAwait(true);
    }

    [Fact]
    public async Task WorklistListIncludesRequestingPhysicianName()
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        await InsertWorklistItemAsync(connection, "SC-PHY-001").ConfigureAwait(true);
        await InsertPhysicianMappingAsync(connection, "Ana Lima").ConfigureAwait(true);

        IReadOnlyCollection<WorklistItemSummaryDto> result = await LabAnalysisModule
            .ExecuteQueryAsync(new GetWorklistItemListQuery())
            .ConfigureAwait(true);

        WorklistItemSummaryDto item = result.Should().ContainSingle(x => x.Id == WorklistItemId).Subject;
        item.RequestedByName.Should().Be("Ana Lima");
    }

    [Fact]
    public async Task WorklistListKeepsTheItemWhenNoPhysicianMappingExists()
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        await InsertWorklistItemAsync(connection, "SC-PHY-002").ConfigureAwait(true);

        IReadOnlyCollection<WorklistItemSummaryDto> result = await LabAnalysisModule
            .ExecuteQueryAsync(new GetWorklistItemListQuery())
            .ConfigureAwait(true);

        WorklistItemSummaryDto item = result.Should().ContainSingle(x => x.Id == WorklistItemId).Subject;
        item.RequestedByName.Should().BeNull();
    }

    [Fact]
    public async Task WorklistDetailIncludesRequestingPhysicianName()
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        await InsertWorklistItemAsync(connection, "SC-PHY-003").ConfigureAwait(true);
        await InsertPhysicianMappingAsync(connection, "Ana Lima").ConfigureAwait(true);

        WorklistItemDetailsDto? dto = await LabAnalysisModule
            .ExecuteQueryAsync(new GetWorklistItemDetailsQuery(WorklistItemId))
            .ConfigureAwait(true);

        dto.Should().NotBeNull();
        dto!.RequestedByName.Should().Be("Ana Lima");
    }

    [Fact]
    public async Task WorklistDetailKeepsTheItemWhenNoPhysicianMappingExists()
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        await InsertWorklistItemAsync(connection, "SC-PHY-004").ConfigureAwait(true);

        WorklistItemDetailsDto? dto = await LabAnalysisModule
            .ExecuteQueryAsync(new GetWorklistItemDetailsQuery(WorklistItemId))
            .ConfigureAwait(true);

        dto.Should().NotBeNull();
        dto!.RequestedByName.Should().BeNull();
    }
}
