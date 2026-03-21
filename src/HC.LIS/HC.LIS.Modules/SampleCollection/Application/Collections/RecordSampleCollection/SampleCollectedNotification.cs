using HC.Core.Application.Events;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.RecordSampleCollection;

public class SampleCollectedNotification(SampleCollectedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<SampleCollectedDomainEvent>(domainEvent, id)
{
}
