using HC.Core.Application.Events;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.AddExamToCollection;

public class SampleCreatedForExamNotification(SampleCreatedForExamDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<SampleCreatedForExamDomainEvent>(domainEvent, id)
{
}
