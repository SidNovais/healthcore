using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.DeactivatePhysician;

public class PhysicianDeactivatedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<PhysicianDeactivatedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(
        PhysicianDeactivatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
