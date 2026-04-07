using HC.LIS.Modules.LabAnalysis.Domain.SignedReports;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.SignedReports;

internal static class SignedReportFactory
{
    public static SignedReport Create() =>
        SignedReport.Create(
            SignedReportSampleData.ReportId,
            WorklistItemForSigning.From(
                SignedReportSampleData.WorklistItemId,
                SignedReportSampleData.OrderId,
                SignedReportSampleData.OrderItemId,
                WorklistItemStatus.ReportGenerated,
                SignedReportSampleData.AnalyteSnapshots),
            SignedReportSampleData.Signature,
            SignedReportSampleData.SignedBy,
            SignedReportSampleData.CreatedAt
        );

    public static SignedReport CreateWithHtml()
    {
        SignedReport report = Create();
        report.HtmlUploaded(SignedReportSampleData.HtmlReportPath, SignedReportSampleData.HtmlUploadedAt);
        return report;
    }

    public static SignedReport CreateWithHtmlAndPdf()
    {
        SignedReport report = CreateWithHtml();
        report.PdfUploaded(SignedReportSampleData.PdfReportPath, SignedReportSampleData.PdfUploadedAt);
        return report;
    }
}
