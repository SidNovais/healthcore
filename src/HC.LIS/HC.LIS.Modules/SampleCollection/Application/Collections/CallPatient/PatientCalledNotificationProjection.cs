using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.CallPatient;

public class PatientCalledNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<PatientCalledNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(PatientCalledNotification notification, CancellationToken cancellationToken)
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
