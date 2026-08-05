using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.ReactivatePhysician;

public class PhysicianReactivatedPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<PhysicianReactivatedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        PhysicianReactivatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new PhysicianReactivatedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.PhysicianId,
            notification.DomainEvent.ReactivatedAt
        )).ConfigureAwait(false);
    }
}
