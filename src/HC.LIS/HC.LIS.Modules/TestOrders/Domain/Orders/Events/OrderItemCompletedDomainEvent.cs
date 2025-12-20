using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Events;

public class OrderItemCompletedDomainEvent(
  Guid orderItemId,
  DateTime completedAt
) : DomainEvent
{
    public Guid OrderItemId { get; } = orderItemId;
    public DateTime CompletedAt { get; } = completedAt;
}
