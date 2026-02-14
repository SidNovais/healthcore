using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CancelExam;

public class ExamCanceledNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<ExamCanceledNotification>
{
    private readonly IList<IProjector> _projectors = projectors;
    public async Task Handle(
        ExamCanceledNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
