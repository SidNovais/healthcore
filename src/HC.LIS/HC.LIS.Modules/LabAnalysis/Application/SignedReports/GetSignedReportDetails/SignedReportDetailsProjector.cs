using System.Threading.Tasks;
using Dapper;
using HC.Core.Application.Projections;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.LabAnalysis.Domain.SignedReports;
using HC.LIS.Modules.LabAnalysis.Domain.SignedReports.Events;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.GetSignedReportDetails;

internal class SignedReportDetailsProjector(
    ISqlConnectionFactory sqlConnectionFactory
) : ProjectorBase, IProjector
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task Project(IDomainEvent @event)
    {
        await When((dynamic)@event);
    }

    private async Task When(SignedReportCreatedDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"INSERT INTO lab_analysis.signed_report_details
              (id, worklist_item_id, order_id, order_item_id, signature, signed_by, status, created_at)
              VALUES (@ReportId, @WorklistItemId, @OrderId, @OrderItemId, @Signature, @SignedBy, @Status, @CreatedAt)",
            new
            {
                e.ReportId,
                e.WorklistItemId,
                e.OrderId,
                e.OrderItemId,
                e.Signature,
                e.SignedBy,
                Status = SignedReportStatus.Created.Value,
                e.CreatedAt
            }
        ).ConfigureAwait(false);
    }

    private async Task When(HtmlReportUploadedDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE lab_analysis.signed_report_details
              SET html_report_path = @HtmlReportPath, status = @Status
              WHERE id = @ReportId",
            new
            {
                e.HtmlReportPath,
                Status = SignedReportStatus.HtmlUploaded.Value,
                ReportId = e.ReportId
            }
        ).ConfigureAwait(false);
    }

    private async Task When(PdfReportUploadedDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE lab_analysis.signed_report_details
              SET pdf_report_path = @PdfReportPath, status = @Status
              WHERE id = @ReportId",
            new
            {
                e.PdfReportPath,
                Status = SignedReportStatus.PdfUploaded.Value,
                ReportId = e.ReportId
            }
        ).ConfigureAwait(false);
    }

    private static new Task When(IDomainEvent _) => Task.CompletedTask;
}
