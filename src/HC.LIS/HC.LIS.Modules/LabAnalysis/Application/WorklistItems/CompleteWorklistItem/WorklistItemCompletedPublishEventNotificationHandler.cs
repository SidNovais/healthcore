using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.LabAnalysis.IntegrationEvents;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CompleteWorklistItem;

public class WorklistItemCompletedPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<WorklistItemCompletedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        WorklistItemCompletedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new WorklistItemCompletedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.WorklistItemId,
            notification.DomainEvent.SampleId,
            notification.DomainEvent.ExamCode,
            notification.DomainEvent.CompletionType,
            notification.DomainEvent.OrderId,
            notification.DomainEvent.OrderItemId
        )).ConfigureAwait(false);
    }
}
