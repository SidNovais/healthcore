using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.PatientManagement.IntegrationEvents;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.RegisterPatient;

public class PatientRegisteredPublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<PatientRegisteredNotification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        PatientRegisteredNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new PatientRegisteredIntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.PatientId,
            notification.DomainEvent.FullName,
            notification.DomainEvent.DateOfBirth,
            notification.DomainEvent.Gender,
            notification.DomainEvent.MothersFullName,
            notification.DomainEvent.DocumentId,
            notification.DomainEvent.Phone,
            notification.DomainEvent.Email,
            notification.DomainEvent.RegisteredAt
        )).ConfigureAwait(false);
    }
}
