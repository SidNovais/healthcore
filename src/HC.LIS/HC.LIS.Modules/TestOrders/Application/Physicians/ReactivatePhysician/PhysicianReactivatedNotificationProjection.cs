using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.ReactivatePhysician;

public class PhysicianReactivatedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<PhysicianReactivatedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(
        PhysicianReactivatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
