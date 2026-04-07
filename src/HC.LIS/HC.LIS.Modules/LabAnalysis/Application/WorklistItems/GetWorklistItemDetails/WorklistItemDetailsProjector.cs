using System;
using System.Threading.Tasks;
using Dapper;
using HC.Core.Application.Projections;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;
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
              (id, sample_id, sample_barcode, exam_code, patient_id, order_id, order_item_id, status, created_at)
              VALUES (@WorklistItemId, @SampleId, @SampleBarcode, @ExamCode, @PatientId, @OrderId, @OrderItemId, @Status, @CreatedAt)",
            new
            {
                e.WorklistItemId,
                e.SampleId,
                e.SampleBarcode,
                e.ExamCode,
                e.PatientId,
                e.OrderId,
                e.OrderItemId,
                Status = "Pending",
                CreatedAt = e.CreatedAt
            }
        ).ConfigureAwait(false);
    }

    private async Task When(AnalysisResultRecordedDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        bool isOutOfRange = ReferenceRange.Of(e.ReferenceRange).IsOutOfRange(e.ResultValue);
        await connection.ExecuteScalarAsync(
            @"INSERT INTO lab_analysis.worklist_item_analyte_results
              (id, worklist_item_id, analyte_code, result_value, result_unit, reference_range, is_out_of_range, performed_by_id, recorded_at)
              VALUES (@Id, @WorklistItemId, @AnalyteCode, @ResultValue, @ResultUnit, @ReferenceRange, @IsOutOfRange, @PerformedById, @RecordedAt)",
            new
            {
                Id = Guid.CreateVersion7(),
                e.WorklistItemId,
                e.AnalyteCode,
                e.ResultValue,
                e.ResultUnit,
                e.ReferenceRange,
                IsOutOfRange = isOutOfRange,
                e.PerformedById,
                e.RecordedAt
            }
        ).ConfigureAwait(false);

        await connection.ExecuteScalarAsync(
            @"UPDATE lab_analysis.worklist_item_details
              SET status = 'ResultReceived'
              WHERE id = @WorklistItemId AND status = 'Pending'",
            new { e.WorklistItemId }
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
