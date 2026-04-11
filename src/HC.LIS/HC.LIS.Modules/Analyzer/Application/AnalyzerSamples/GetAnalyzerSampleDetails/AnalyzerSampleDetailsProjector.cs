using Dapper;
using HC.Core.Application.Projections;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Events;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleDetails;

internal class AnalyzerSampleDetailsProjector(
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
        await connection.ExecuteScalarAsync(
            @"INSERT INTO analyzer.analyzer_sample_details
              (id, sample_id, patient_id, sample_barcode, patient_name, patient_birthdate, patient_gender, is_urgent, status, created_at)
              VALUES (@AnalyzerSampleId, @SampleId, @PatientId, @SampleBarcode, @PatientName, @PatientBirthdate, @PatientGender, @IsUrgent, @Status, @CreatedAt)",
            new
            {
                e.AnalyzerSampleId,
                e.SampleId,
                e.PatientId,
                e.SampleBarcode,
                e.PatientName,
                e.PatientBirthdate,
                e.PatientGender,
                e.IsUrgent,
                Status = "AwaitingQuery",
                e.CreatedAt
            }
        ).ConfigureAwait(false);
    }

    private async Task When(SampleInfoDispatchedDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE analyzer.analyzer_sample_details
              SET status = @Status, dispatched_at = @DispatchedAt
              WHERE id = @AnalyzerSampleId",
            new
            {
                Status = "InfoDispatched",
                e.DispatchedAt,
                e.AnalyzerSampleId
            }
        ).ConfigureAwait(false);
    }

    private async Task When(ExamResultReceivedDomainEvent e)
    {
        if (!e.AllResultsReceived)
            return;

        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE analyzer.analyzer_sample_details
              SET status = @Status
              WHERE id = @AnalyzerSampleId",
            new
            {
                Status = "ResultReceived",
                e.AnalyzerSampleId
            }
        ).ConfigureAwait(false);
    }

    private static new Task When(IDomainEvent _) => Task.CompletedTask;
}
