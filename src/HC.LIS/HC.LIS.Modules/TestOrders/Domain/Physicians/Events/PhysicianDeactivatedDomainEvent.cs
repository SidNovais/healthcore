using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Physicians.Events;

public class PhysicianDeactivatedDomainEvent(
    Guid physicianId,
    DateTime deactivatedAt
) : DomainEvent
{
    public Guid PhysicianId { get; } = physicianId;
    public DateTime DeactivatedAt { get; } = deactivatedAt;
}
