using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.RegisterPhysician;

public class PhysicianRegisteredNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<PhysicianRegisteredNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(
        PhysicianRegisteredNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
