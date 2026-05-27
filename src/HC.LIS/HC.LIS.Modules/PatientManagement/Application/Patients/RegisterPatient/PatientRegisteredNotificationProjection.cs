using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.RegisterPatient;

public class PatientRegisteredNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<PatientRegisteredNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(
        PatientRegisteredNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
