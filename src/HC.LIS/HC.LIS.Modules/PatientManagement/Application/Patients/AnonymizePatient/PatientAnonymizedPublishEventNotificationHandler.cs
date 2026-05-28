using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.PatientManagement.IntegrationEvents;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.AnonymizePatient;

public class PatientAnonymizedPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<PatientAnonymizedNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        PatientAnonymizedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new PatientAnonymizedIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.PatientId,
            notification.DomainEvent.AnonymizedAt
        )).ConfigureAwait(false);
    }
}
