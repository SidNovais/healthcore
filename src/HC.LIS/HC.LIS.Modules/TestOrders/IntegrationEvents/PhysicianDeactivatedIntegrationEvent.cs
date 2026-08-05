using System;
using HC.Core.Infrastructure.EventBus;

namespace HC.LIS.Modules.TestOrders.IntegrationEvents;

public class PhysicianDeactivatedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid physicianId,
    DateTime deactivatedAt
) : IntegrationEvent(id, occurredAt)
{
    public Guid PhysicianId { get; } = physicianId;
    public DateTime DeactivatedAt { get; } = deactivatedAt;
}
