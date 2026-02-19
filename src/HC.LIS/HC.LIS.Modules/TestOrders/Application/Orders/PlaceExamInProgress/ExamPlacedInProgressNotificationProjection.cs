using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamInProgress;

public class ExamPlacedInProgressNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<ExamPlacedInProgressNotification>
{
    private readonly IList<IProjector> _projectors = projectors;
    public async Task Handle(
        ExamPlacedInProgressNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
