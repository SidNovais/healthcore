using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.UpdatePatient;

public class PatientUpdatedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<PatientUpdatedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(
        PatientUpdatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
