using HC.Core.Application.Events;
using HC.LIS.Modules.TestOrders.Domain.Physicians.Events;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.ReactivatePhysician;

public class PhysicianReactivatedNotification(PhysicianReactivatedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<PhysicianReactivatedDomainEvent>(domainEvent, id)
{

}
