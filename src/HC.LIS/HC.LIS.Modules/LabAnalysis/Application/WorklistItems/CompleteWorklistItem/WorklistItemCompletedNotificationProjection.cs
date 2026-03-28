using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CompleteWorklistItem;

public class WorklistItemCompletedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<WorklistItemCompletedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(WorklistItemCompletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
