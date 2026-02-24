using System;
using HC.COre.Infrastructure.EventBus;

namespace HC.LIS.Modules.TestOrders.IntegrationEvents;

public class OrderItemCanceledIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid orderItemId
) : IntegrationEvent(id, occurredAt)
{
    public Guid OrderItemId { get; } = orderItemId;
}
