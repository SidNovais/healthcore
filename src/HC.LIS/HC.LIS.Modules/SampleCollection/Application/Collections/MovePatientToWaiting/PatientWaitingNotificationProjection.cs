using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.MovePatientToWaiting;

public class PatientWaitingNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<PatientWaitingNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(PatientWaitingNotification notification, CancellationToken cancellationToken)
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
