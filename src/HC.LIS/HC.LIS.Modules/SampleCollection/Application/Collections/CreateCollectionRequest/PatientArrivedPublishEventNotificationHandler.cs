using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.SampleCollection.IntegrationEvents;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.CreateCollectionRequest;

public class PatientArrivedPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<PatientArrivedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        PatientArrivedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new PatientArrivedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.CollectionRequestId,
            notification.DomainEvent.PatientId
        )).ConfigureAwait(false);
    }
}
