using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using Npgsql;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemList;

namespace HC.LIS.Modules.LabAnalysis.IntegrationTests.WorklistItems;

public class WorklistItemPatientInfoTests : TestBase
{
    private static readonly Guid WorklistItemId = Guid.Parse("019b664c-0000-7f37-a794-000000000010");
    private static readonly Guid PatientId      = Guid.Parse("019b664c-0000-7f37-a794-000000000011");
    private static readonly DateTime DateOfBirth = new(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc);

    public WorklistItemPatientInfoTests() : base(Guid.CreateVersion7()) { }

    [Fact]
    public async Task WorklistListIncludesPatientNameWhenSnapshotExists()
    {
        using var connection = new NpgsqlConnection(ConnectionString);

        await connection.ExecuteAsync(
            """
            INSERT INTO lab_analysis.worklist_item_details
                (id, sample_id, sample_barcode, exam_code, patient_id, order_id, order_item_id, status, created_at)
            VALUES
                (@Id, @SampleId, @SampleBarcode, @ExamCode, @PatientId, @OrderId, @OrderItemId, @Status, @CreatedAt)
            """,
            new
            {
                Id           = WorklistItemId,
                SampleId     = Guid.NewGuid(),
                SampleBarcode = "SC-PAT-001",
                ExamCode     = "GLU",
                PatientId,
                OrderId      = Guid.NewGuid(),
                OrderItemId  = Guid.NewGuid(),
                Status       = "Pending",
                CreatedAt    = DateTime.UtcNow
            }
        ).ConfigureAwait(true);

        await connection.ExecuteAsync(
            """
            INSERT INTO "lab_analysis"."PatientSnapshotDetails"
                ("Id", "FullName", "DateOfBirth", "Gender", "Status", "RegisteredAt")
            VALUES
                (@Id, @FullName, @DateOfBirth, @Gender, @Status, @RegisteredAt)
            """,
            new
            {
                Id           = PatientId,
                FullName     = "John Doe",
                DateOfBirth,
                Gender       = "Male",
                Status       = "Active",
                RegisteredAt = DateTime.UtcNow
            }
        ).ConfigureAwait(true);

        IReadOnlyCollection<WorklistItemSummaryDto> result = await LabAnalysisModule
            .ExecuteQueryAsync(new GetWorklistItemListQuery())
            .ConfigureAwait(true);

        WorklistItemSummaryDto item = result.Should().ContainSingle(x => x.Id == WorklistItemId).Subject;
        item.PatientName.Should().Be("John Doe");
        item.PatientDateOfBirth.Should().Be(DateOfBirth);
        item.PatientGender.Should().Be("Male");
    }

    [Fact]
    public async Task WorklistListPatientFieldsAreNullWhenNoSnapshotExists()
    {
        using var connection = new NpgsqlConnection(ConnectionString);

        await connection.ExecuteAsync(
            """
            INSERT INTO lab_analysis.worklist_item_details
                (id, sample_id, sample_barcode, exam_code, patient_id, order_id, order_item_id, status, created_at)
            VALUES
                (@Id, @SampleId, @SampleBarcode, @ExamCode, @PatientId, @OrderId, @OrderItemId, @Status, @CreatedAt)
            """,
            new
            {
                Id           = WorklistItemId,
                SampleId     = Guid.NewGuid(),
                SampleBarcode = "SC-PAT-002",
                ExamCode     = "GLU",
                PatientId,
                OrderId      = Guid.NewGuid(),
                OrderItemId  = Guid.NewGuid(),
                Status       = "Pending",
                CreatedAt    = DateTime.UtcNow
            }
        ).ConfigureAwait(true);

        IReadOnlyCollection<WorklistItemSummaryDto> result = await LabAnalysisModule
            .ExecuteQueryAsync(new GetWorklistItemListQuery())
            .ConfigureAwait(true);

        WorklistItemSummaryDto item = result.Should().ContainSingle(x => x.Id == WorklistItemId).Subject;
        item.PatientName.Should().BeNull();
        item.PatientDateOfBirth.Should().BeNull();
        item.PatientGender.Should().BeNull();
    }

    [Fact]
    public async Task WorklistDetailIncludesPatientNameWhenSnapshotExists()
    {
        using var connection = new NpgsqlConnection(ConnectionString);

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
                SampleBarcode = "SC-PAT-003",
                ExamCode      = "GLU",
                PatientId,
                OrderId       = Guid.NewGuid(),
                OrderItemId   = Guid.NewGuid(),
                Status        = "Pending",
                CreatedAt     = DateTime.UtcNow
            }
        ).ConfigureAwait(true);

        await connection.ExecuteAsync(
            """
            INSERT INTO "lab_analysis"."PatientSnapshotDetails"
                ("Id", "FullName", "DateOfBirth", "Gender", "Status", "RegisteredAt")
            VALUES
                (@Id, @FullName, @DateOfBirth, @Gender, @Status, @RegisteredAt)
            """,
            new
            {
                Id           = PatientId,
                FullName     = "John Doe",
                DateOfBirth,
                Gender       = "Male",
                Status       = "Active",
                RegisteredAt = DateTime.UtcNow
            }
        ).ConfigureAwait(true);

        WorklistItemDetailsDto? dto = await LabAnalysisModule
            .ExecuteQueryAsync(new GetWorklistItemDetailsQuery(WorklistItemId))
            .ConfigureAwait(true);

        dto.Should().NotBeNull();
        dto!.PatientName.Should().Be("John Doe");
        dto.PatientDateOfBirth.Should().Be(DateOfBirth);
        dto.PatientGender.Should().Be("Male");
    }
}
