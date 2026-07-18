using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.SampleCollection.IntegrationEvents;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.MovePatientToWaiting;

public class PatientWaitingPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<PatientWaitingNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        PatientWaitingNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new PatientWaitingIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.CollectionRequestId,
            notification.DomainEvent.PatientId
        )).ConfigureAwait(false);
    }
}
