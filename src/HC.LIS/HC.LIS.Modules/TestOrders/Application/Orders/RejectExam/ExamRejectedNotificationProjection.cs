using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.TestOrders.Application.Orders.RejectExam;

public class ExamRejectedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<ExamRejectedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;
    public async Task Handle(
        ExamRejectedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
