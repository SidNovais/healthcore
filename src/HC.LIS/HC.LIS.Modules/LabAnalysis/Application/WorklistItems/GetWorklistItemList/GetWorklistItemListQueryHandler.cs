using System.Collections.Generic;
using System.Data;
using Dapper;
using HC.Core.Application.Queries;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Queries;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemList;

internal class GetWorklistItemListQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetWorklistItemListQuery, IReadOnlyCollection<WorklistItemSummaryDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<IReadOnlyCollection<WorklistItemSummaryDto>> Handle(
        GetWorklistItemListQuery query,
        CancellationToken cancellationToken)
    {
        const string baseSql = $"""
            SELECT
                wid.id             AS "{nameof(WorklistItemSummaryDto.Id)}",
                wid.sample_barcode AS "{nameof(WorklistItemSummaryDto.SampleBarcode)}",
                wid.exam_code      AS "{nameof(WorklistItemSummaryDto.ExamCode)}",
                wid.patient_id     AS "{nameof(WorklistItemSummaryDto.PatientId)}",
                psd."FullName"     AS "{nameof(WorklistItemSummaryDto.PatientName)}",
                psd."DateOfBirth"  AS "{nameof(WorklistItemSummaryDto.PatientDateOfBirth)}",
                psd."Gender"       AS "{nameof(WorklistItemSummaryDto.PatientGender)}",
                wid.status         AS "{nameof(WorklistItemSummaryDto.Status)}",
                wid.created_at     AS "{nameof(WorklistItemSummaryDto.CreatedAt)}"
            FROM lab_analysis.worklist_item_details AS wid
            LEFT JOIN lab_analysis."PatientSnapshotDetails" AS psd ON psd."Id" = wid.patient_id
            WHERE (@Status IS NULL OR wid.status = @Status)
            ORDER BY wid.created_at
            """;

        string sql = PagedQueryHelper.AppendPageStatement(baseSql);
        PageData pageData = PagedQueryHelper.GetPageData(query);

        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Database connection is unavailable.");

        IEnumerable<WorklistItemSummaryDto> results = await connection.QueryAsync<WorklistItemSummaryDto>(
            sql, new { query.Status, pageData.Offset, pageData.Next }
        ).ConfigureAwait(false);

        return results.AsList().AsReadOnly();
    }
}
