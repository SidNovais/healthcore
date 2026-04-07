using System;
using HC.Core.Application.Events;
using HC.LIS.Modules.LabAnalysis.Domain.SignedReports.Events;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.SignReport;

public class SignedReportCreatedNotification(SignedReportCreatedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<SignedReportCreatedDomainEvent>(domainEvent, id)
{
}
