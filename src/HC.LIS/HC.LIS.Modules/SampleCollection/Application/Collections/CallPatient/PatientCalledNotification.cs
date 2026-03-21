using HC.Core.Application.Events;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.CallPatient;

public class PatientCalledNotification(PatientCalledDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<PatientCalledDomainEvent>(domainEvent, id)
{
}
