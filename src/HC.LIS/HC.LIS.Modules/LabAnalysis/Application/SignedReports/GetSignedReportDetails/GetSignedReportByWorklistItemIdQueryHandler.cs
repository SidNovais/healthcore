using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Queries;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.GetSignedReportDetails;

internal class GetSignedReportByWorklistItemIdQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetSignedReportByWorklistItemIdQuery, SignedReportDetailsDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<SignedReportDetailsDto?> Handle(
        GetSignedReportByWorklistItemIdQuery query,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                sr.id                AS ""Id"",
                sr.worklist_item_id  AS ""WorklistItemId"",
                sr.order_id          AS ""OrderId"",
                sr.order_item_id     AS ""OrderItemId"",
                sr.html_report_path  AS ""HtmlReportPath"",
                sr.pdf_report_path   AS ""PdfReportPath"",
                sr.signature         AS ""Signature"",
                sr.signed_by         AS ""SignedBy"",
                sr.status            AS ""Status"",
                sr.created_at        AS ""CreatedAt""
            FROM lab_analysis.signed_report_details AS sr
            WHERE sr.worklist_item_id = @WorklistItemId;";

        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to get signed report details");

        return await connection
            .QueryFirstOrDefaultAsync<SignedReportDetailsDto>(sql, new { query.WorklistItemId })
            .ConfigureAwait(false);
    }
}
