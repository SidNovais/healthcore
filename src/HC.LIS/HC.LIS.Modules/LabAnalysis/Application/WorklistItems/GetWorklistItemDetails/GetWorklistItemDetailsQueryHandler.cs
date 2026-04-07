using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        const string sql = @"
            SELECT
                wid.id               AS ""Id"",
                wid.sample_id        AS ""SampleId"",
                wid.sample_barcode   AS ""SampleBarcode"",
                wid.exam_code        AS ""ExamCode"",
                wid.patient_id       AS ""PatientId"",
                wid.order_id         AS ""OrderId"",
                wid.order_item_id    AS ""OrderItemId"",
                wid.status           AS ""Status"",
                wid.report_path      AS ""ReportPath"",
                wid.completion_type  AS ""CompletionType"",
                wid.created_at       AS ""CreatedAt"",
                wid.completed_at     AS ""CompletedAt""
            FROM lab_analysis.worklist_item_details AS wid
            WHERE wid.id = @WorklistItemId;

            SELECT
                r.id              AS ""Id"",
                r.analyte_code    AS ""AnalyteCode"",
                r.result_value    AS ""ResultValue"",
                r.result_unit     AS ""ResultUnit"",
                r.reference_range  AS ""ReferenceRange"",
                r.is_out_of_range  AS ""IsOutOfRange"",
                r.performed_by_id  AS ""PerformedById"",
                r.recorded_at     AS ""RecordedAt""
            FROM lab_analysis.worklist_item_analyte_results AS r
            WHERE r.worklist_item_id = @WorklistItemId
            ORDER BY r.recorded_at;";

        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to get worklist item details");

        using SqlMapper.GridReader multi = await connection
            .QueryMultipleAsync(sql, new { query.WorklistItemId })
            .ConfigureAwait(false);

        WorklistItemDetailsDto? dto = await multi
            .ReadFirstOrDefaultAsync<WorklistItemDetailsDto>()
            .ConfigureAwait(false);

        if (dto is null)
            return null;

        IEnumerable<AnalyteResultDto> analyteResults = await multi
            .ReadAsync<AnalyteResultDto>()
            .ConfigureAwait(false);

        dto.AnalyteResults = analyteResults.ToList().AsReadOnly();
        return dto;
    }
}
