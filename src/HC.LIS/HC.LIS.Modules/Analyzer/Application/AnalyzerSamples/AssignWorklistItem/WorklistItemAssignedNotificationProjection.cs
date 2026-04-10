using HC.Core.Application.Projections;
using MediatR;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.AssignWorklistItem;

public class WorklistItemAssignedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<WorklistItemAssignedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(
        WorklistItemAssignedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
