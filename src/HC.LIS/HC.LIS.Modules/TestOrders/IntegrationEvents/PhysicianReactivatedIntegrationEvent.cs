using System;
using HC.Core.Infrastructure.EventBus;

namespace HC.LIS.Modules.TestOrders.IntegrationEvents;

public class PhysicianReactivatedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid physicianId,
    DateTime reactivatedAt
) : IntegrationEvent(id, occurredAt)
{
    public Guid PhysicianId { get; } = physicianId;
    public DateTime ReactivatedAt { get; } = reactivatedAt;
}
