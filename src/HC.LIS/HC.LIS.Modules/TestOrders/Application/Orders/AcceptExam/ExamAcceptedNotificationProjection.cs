using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.TestOrders.Application.Orders.AcceptExam;

public class ExamAcceptedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<ExamAcceptedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;
    public async Task Handle(
        ExamAcceptedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
