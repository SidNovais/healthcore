using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CreateOrder;

public class OrderCreatedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<OrderCreatedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;
    public async Task Handle(
        OrderCreatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
