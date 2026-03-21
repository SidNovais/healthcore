using HC.Core.Application.Events;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.CreateCollectionRequest;

public class PatientArrivedNotification(PatientArrivedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<PatientArrivedDomainEvent>(domainEvent, id)
{
}
