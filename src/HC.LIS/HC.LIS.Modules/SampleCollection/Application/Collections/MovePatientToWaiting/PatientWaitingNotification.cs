using HC.Core.Application.Events;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.MovePatientToWaiting;

public class PatientWaitingNotification(PatientWaitingDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<PatientWaitingDomainEvent>(domainEvent, id)
{
}
