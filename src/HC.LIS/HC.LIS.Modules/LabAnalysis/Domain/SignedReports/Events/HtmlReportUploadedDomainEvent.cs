using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.SignedReports.Events;

public class HtmlReportUploadedDomainEvent(
    Guid reportId,
    Guid worklistItemId,
    string htmlReportPath,
    DateTime uploadedAt
) : DomainEvent
{
    public Guid ReportId { get; } = reportId;
    public Guid WorklistItemId { get; } = worklistItemId;
    public string HtmlReportPath { get; } = htmlReportPath;
    public DateTime UploadedAt { get; } = uploadedAt;
}
