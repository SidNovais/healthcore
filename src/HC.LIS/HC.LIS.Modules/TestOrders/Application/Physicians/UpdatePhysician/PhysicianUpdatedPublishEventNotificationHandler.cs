using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.UpdatePhysician;

public class PhysicianUpdatedPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<PhysicianUpdatedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        PhysicianUpdatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new PhysicianUpdatedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.PhysicianId,
            notification.DomainEvent.FullName,
            notification.DomainEvent.LicenceNumber,
            notification.DomainEvent.UpdatedAt
        )).ConfigureAwait(false);
    }
}
