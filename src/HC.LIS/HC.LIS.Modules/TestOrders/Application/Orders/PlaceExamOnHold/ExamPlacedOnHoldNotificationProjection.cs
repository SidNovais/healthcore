using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamOnHold;

public class ExamPlacedOnHoldNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<ExamPlacedOnHoldNotification>
{
    private readonly IList<IProjector> _projectors = projectors;
    public async Task Handle(
        ExamPlacedOnHoldNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
