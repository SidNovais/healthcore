using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.DeactivatePhysician;

public class PhysicianDeactivatedPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<PhysicianDeactivatedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        PhysicianDeactivatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new PhysicianDeactivatedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.PhysicianId,
            notification.DomainEvent.DeactivatedAt
        )).ConfigureAwait(false);
    }
}
