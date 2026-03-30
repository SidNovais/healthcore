using System.Threading.Tasks;
using Dapper;
using HC.Core.Application.Projections;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;

internal class WorklistItemDetailsProjector(
    ISqlConnectionFactory sqlConnectionFactory
) : ProjectorBase, IProjector
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task Project(IDomainEvent @event)
    {
        await When((dynamic)@event);
    }

    private async Task When(WorklistItemCreatedDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"INSERT INTO lab_analysis.worklist_item_details
              (id, sample_id, sample_barcode, exam_code, patient_id, status, created_at)
              VALUES (@WorklistItemId, @SampleId, @SampleBarcode, @ExamCode, @PatientId, @Status, @CreatedAt)",
            new
            {
                e.WorklistItemId,
                e.SampleId,
                e.SampleBarcode,
                e.ExamCode,
                e.PatientId,
                Status = "Pending",
                CreatedAt = e.CreatedAt
            }
        ).ConfigureAwait(false);
    }

    private async Task When(AnalysisResultRecordedDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE lab_analysis.worklist_item_details
              SET result_value = @ResultValue, result_unit = @ResultUnit, reference_range = @ReferenceRange, status = @Status
              WHERE id = @WorklistItemId",
            new
            {
                e.ResultValue,
                e.ResultUnit,
                e.ReferenceRange,
                Status = "ResultReceived",
                e.WorklistItemId
            }
        ).ConfigureAwait(false);
    }

    private async Task When(ReportGeneratedDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE lab_analysis.worklist_item_details
              SET report_path = @ReportPath, status = @Status
              WHERE id = @WorklistItemId",
            new
            {
                e.ReportPath,
                Status = "ReportGenerated",
                e.WorklistItemId
            }
        ).ConfigureAwait(false);
    }

    private async Task When(WorklistItemCompletedDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE lab_analysis.worklist_item_details
              SET status = @Status, completion_type = @CompletionType, completed_at = @CompletedAt
              WHERE id = @WorklistItemId",
            new
            {
                Status = "Completed",
                e.CompletionType,
                e.CompletedAt,
                e.WorklistItemId
            }
        ).ConfigureAwait(false);
    }

    private static new Task When(IDomainEvent _) => Task.CompletedTask;
}
