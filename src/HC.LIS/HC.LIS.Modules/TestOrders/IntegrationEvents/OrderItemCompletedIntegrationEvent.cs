using System;
using HC.Core.Infrastructure.EventBus;

namespace HC.LIS.Modules.TestOrders.IntegrationEvents;

public class OrderItemCompletedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid orderItemId
) : IntegrationEvent(id, occurredAt)
{
    public Guid OrderItemId { get; } = orderItemId;
}
