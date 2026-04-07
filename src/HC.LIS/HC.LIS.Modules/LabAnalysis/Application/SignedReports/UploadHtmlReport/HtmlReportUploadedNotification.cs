using System;
using HC.Core.Application.Events;
using HC.LIS.Modules.LabAnalysis.Domain.SignedReports.Events;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.UploadHtmlReport;

public class HtmlReportUploadedNotification(HtmlReportUploadedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<HtmlReportUploadedDomainEvent>(domainEvent, id)
{
}
