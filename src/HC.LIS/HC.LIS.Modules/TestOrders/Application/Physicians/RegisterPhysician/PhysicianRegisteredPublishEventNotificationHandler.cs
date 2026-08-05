using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.RegisterPhysician;

public class PhysicianRegisteredPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<PhysicianRegisteredNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        PhysicianRegisteredNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new PhysicianRegisteredIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.PhysicianId,
            notification.DomainEvent.FullName,
            notification.DomainEvent.LicenceNumber,
            notification.DomainEvent.RegisteredAt
        )).ConfigureAwait(false);
    }
}
