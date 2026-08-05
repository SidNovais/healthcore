using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.UpdatePhysician;

public class PhysicianUpdatedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<PhysicianUpdatedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(
        PhysicianUpdatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
