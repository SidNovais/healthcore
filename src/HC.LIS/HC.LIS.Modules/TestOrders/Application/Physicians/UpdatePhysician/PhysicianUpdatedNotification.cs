using HC.Core.Application.Events;
using HC.LIS.Modules.TestOrders.Domain.Physicians.Events;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.UpdatePhysician;

public class PhysicianUpdatedNotification(PhysicianUpdatedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<PhysicianUpdatedDomainEvent>(domainEvent, id)
{

}
