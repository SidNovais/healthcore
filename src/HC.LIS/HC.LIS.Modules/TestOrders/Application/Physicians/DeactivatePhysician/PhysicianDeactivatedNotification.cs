using HC.Core.Application.Events;
using HC.LIS.Modules.TestOrders.Domain.Physicians.Events;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.DeactivatePhysician;

public class PhysicianDeactivatedNotification(PhysicianDeactivatedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<PhysicianDeactivatedDomainEvent>(domainEvent, id)
{

}
