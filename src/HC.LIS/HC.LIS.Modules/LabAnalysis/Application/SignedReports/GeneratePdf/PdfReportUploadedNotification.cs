using System;
using HC.Core.Application.Events;
using HC.LIS.Modules.LabAnalysis.Domain.SignedReports.Events;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.GeneratePdf;

public class PdfReportUploadedNotification(PdfReportUploadedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<PdfReportUploadedDomainEvent>(domainEvent, id)
{
}
