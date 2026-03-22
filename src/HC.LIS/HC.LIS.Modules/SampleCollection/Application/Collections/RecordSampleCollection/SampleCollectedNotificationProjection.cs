using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.RecordSampleCollection;

public class SampleCollectedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<SampleCollectedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(SampleCollectedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
