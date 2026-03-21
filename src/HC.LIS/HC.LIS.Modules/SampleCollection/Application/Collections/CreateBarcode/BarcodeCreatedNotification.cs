using HC.Core.Application.Events;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.CreateBarcode;

public class BarcodeCreatedNotification(BarcodeCreatedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<BarcodeCreatedDomainEvent>(domainEvent, id)
{
}
