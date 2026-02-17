using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CompleteExam;

public class ExamCompletedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<ExamCompletedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;
    public async Task Handle(
        ExamCompletedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
