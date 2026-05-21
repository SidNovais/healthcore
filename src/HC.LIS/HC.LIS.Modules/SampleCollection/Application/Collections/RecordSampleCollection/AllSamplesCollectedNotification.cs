using System;
using HC.Core.Application.Events;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.RecordSampleCollection;

public class AllSamplesCollectedNotification(AllSamplesCollectedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<AllSamplesCollectedDomainEvent>(domainEvent, id)
{
}
