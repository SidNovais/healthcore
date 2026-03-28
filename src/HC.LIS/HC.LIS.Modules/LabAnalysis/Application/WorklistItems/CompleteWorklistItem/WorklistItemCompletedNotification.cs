using System;
using HC.Core.Application.Events;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CompleteWorklistItem;

public class WorklistItemCompletedNotification(WorklistItemCompletedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<WorklistItemCompletedDomainEvent>(domainEvent, id)
{
}
