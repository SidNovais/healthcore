using Dapper;
using HC.Core.Application.Projections;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Events;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleExamDetails;

internal class AnalyzerSampleExamDetailsProjector(
    ISqlConnectionFactory sqlConnectionFactory
) : ProjectorBase, IProjector
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task Project(IDomainEvent @event)
    {
        await When((dynamic)@event);
    }

    private async Task When(AnalyzerSampleCreatedDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        foreach (string examMnemonic in e.ExamMnemonics)
        {
            await connection.ExecuteScalarAsync(
                @"INSERT INTO analyzer.analyzer_sample_exam_details
                  (id, analyzer_sample_id, exam_mnemonic)
                  VALUES (@Id, @AnalyzerSampleId, @ExamMnemonic)",
                new
                {
                    Id = Guid.CreateVersion7(),
                    e.AnalyzerSampleId,
                    ExamMnemonic = examMnemonic
                }
            ).ConfigureAwait(false);
        }
    }

    private async Task When(WorklistItemAssignedDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE analyzer.analyzer_sample_exam_details
              SET worklist_item_id = @WorklistItemId
              WHERE analyzer_sample_id = @AnalyzerSampleId AND exam_mnemonic = @ExamMnemonic",
            new
            {
                e.WorklistItemId,
                e.AnalyzerSampleId,
                e.ExamMnemonic
            }
        ).ConfigureAwait(false);
    }

    private async Task When(ExamResultReceivedDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE analyzer.analyzer_sample_exam_details
              SET result_value = @ResultValue,
                  result_unit = @ResultUnit,
                  reference_range = @ReferenceRange,
                  instrument_id = @InstrumentId,
                  recorded_at = @RecordedAt
              WHERE analyzer_sample_id = @AnalyzerSampleId AND exam_mnemonic = @ExamMnemonic",
            new
            {
                e.ResultValue,
                e.ResultUnit,
                e.ReferenceRange,
                e.InstrumentId,
                e.RecordedAt,
                e.AnalyzerSampleId,
                e.ExamMnemonic
            }
        ).ConfigureAwait(false);
    }

    private static new Task When(IDomainEvent _) => Task.CompletedTask;
}
