using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PartiallyCompleteExam;

public class ExamPartiallyCompletedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<ExamPartiallyCompletedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;
    public async Task Handle(
        ExamPartiallyCompletedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
