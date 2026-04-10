using HC.Core.Application.Events;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Events;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.AssignWorklistItem;

public class WorklistItemAssignedNotification(WorklistItemAssignedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<WorklistItemAssignedDomainEvent>(domainEvent, id)
{
}
