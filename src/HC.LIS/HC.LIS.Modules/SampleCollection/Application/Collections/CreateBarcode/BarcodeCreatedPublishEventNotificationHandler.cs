using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.SampleCollection.IntegrationEvents;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.CreateBarcode;

public class BarcodeCreatedPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<BarcodeCreatedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        BarcodeCreatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new BarcodeCreatedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.CollectionRequestId,
            notification.DomainEvent.SampleId,
            notification.DomainEvent.BarcodeValue
        )).ConfigureAwait(false);
    }
}
