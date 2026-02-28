using System;
using HC.COre.Infrastructure.EventBus;

namespace HC.LIS.Modules.TestOrders.IntegrationEvents;

public class OrderItemRejectedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid orderItemId,
    string reason
) : IntegrationEvent(id, occurredAt)
{
    public Guid OrderItemId { get; } = orderItemId;
    public string Reason { get; } = reason;
}
