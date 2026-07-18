using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.SampleCollection.IntegrationEvents;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.CallPatient;

public class PatientCalledPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<PatientCalledNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        PatientCalledNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new PatientCalledIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.CollectionRequestId,
            notification.DomainEvent.PatientId
        )).ConfigureAwait(false);
    }
}
