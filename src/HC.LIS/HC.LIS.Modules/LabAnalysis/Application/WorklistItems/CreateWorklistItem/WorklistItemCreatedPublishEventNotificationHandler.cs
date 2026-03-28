using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.LabAnalysis.IntegrationEvents;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CreateWorklistItem;

public class WorklistItemCreatedPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<WorklistItemCreatedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        WorklistItemCreatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new WorklistItemCreatedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.WorklistItemId,
            notification.DomainEvent.PatientId,
            notification.DomainEvent.SampleBarcode,
            notification.DomainEvent.ExamCode
        )).ConfigureAwait(false);
    }
}
