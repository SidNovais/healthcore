using System;
using HC.Core.Application.Events;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GenerateReport;

public class ReportGeneratedNotification(ReportGeneratedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<ReportGeneratedDomainEvent>(domainEvent, id)
{
}
