using System;
using HC.Core.Application.Events;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CreateWorklistItem;

public class WorklistItemCreatedNotification(WorklistItemCreatedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<WorklistItemCreatedDomainEvent>(domainEvent, id)
{
}
