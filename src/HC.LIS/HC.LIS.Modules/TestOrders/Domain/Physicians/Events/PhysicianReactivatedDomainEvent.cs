using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Physicians.Events;

public class PhysicianReactivatedDomainEvent(
    Guid physicianId,
    DateTime reactivatedAt
) : DomainEvent
{
    public Guid PhysicianId { get; } = physicianId;
    public DateTime ReactivatedAt { get; } = reactivatedAt;
}
