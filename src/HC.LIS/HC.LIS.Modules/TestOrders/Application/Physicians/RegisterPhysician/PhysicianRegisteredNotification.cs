using HC.Core.Application.Events;
using HC.LIS.Modules.TestOrders.Domain.Physicians.Events;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.RegisterPhysician;

public class PhysicianRegisteredNotification(PhysicianRegisteredDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<PhysicianRegisteredDomainEvent>(domainEvent, id)
{

}
