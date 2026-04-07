using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.SignedReports.Events;

public class PdfReportUploadedDomainEvent(
    Guid reportId,
    Guid worklistItemId,
    string pdfReportPath,
    DateTime uploadedAt
) : DomainEvent
{
    public Guid ReportId { get; } = reportId;
    public Guid WorklistItemId { get; } = worklistItemId;
    public string PdfReportPath { get; } = pdfReportPath;
    public DateTime UploadedAt { get; } = uploadedAt;
}
