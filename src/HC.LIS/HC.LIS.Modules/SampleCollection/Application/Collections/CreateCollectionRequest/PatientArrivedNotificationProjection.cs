using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.CreateCollectionRequest;

public class PatientArrivedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<PatientArrivedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(PatientArrivedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
