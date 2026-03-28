using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Queries;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;

internal class GetWorklistItemDetailsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetWorklistItemDetailsQuery, WorklistItemDetailsDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<WorklistItemDetailsDto?> Handle(
        GetWorklistItemDetailsQuery query,
        CancellationToken cancellationToken
    )
    {
        string sql = @$"SELECT
            wid.id AS ""{nameof(WorklistItemDetailsDto.Id)}"",
            wid.sample_id AS ""{nameof(WorklistItemDetailsDto.SampleId)}"",
            wid.sample_barcode AS ""{nameof(WorklistItemDetailsDto.SampleBarcode)}"",
            wid.exam_code AS ""{nameof(WorklistItemDetailsDto.ExamCode)}"",
            wid.patient_id AS ""{nameof(WorklistItemDetailsDto.PatientId)}"",
            wid.status AS ""{nameof(WorklistItemDetailsDto.Status)}"",
            wid.result_value AS ""{nameof(WorklistItemDetailsDto.ResultValue)}"",
            wid.report_path AS ""{nameof(WorklistItemDetailsDto.ReportPath)}"",
            wid.completion_type AS ""{nameof(WorklistItemDetailsDto.CompletionType)}"",
            wid.created_at AS ""{nameof(WorklistItemDetailsDto.CreatedAt)}"",
            wid.completed_at AS ""{nameof(WorklistItemDetailsDto.CompletedAt)}""
            FROM lab_analysis.worklist_item_details AS wid
            WHERE wid.id = @WorklistItemId";

        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to get worklist item details");

        return await connection.QueryFirstOrDefaultAsync<WorklistItemDetailsDto>(
            sql,
            new { query.WorklistItemId }
        ).ConfigureAwait(false);
    }
}
