using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.AnonymizePatient;

public class PatientAnonymizedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<PatientAnonymizedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(
        PatientAnonymizedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
