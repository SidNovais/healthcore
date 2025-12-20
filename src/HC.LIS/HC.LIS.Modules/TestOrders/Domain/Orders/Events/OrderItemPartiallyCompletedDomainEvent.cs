using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Events;

public class OrderItemPartiallyCompletedDomainEvent(
  Guid orderItemId,
  DateTime partiallyCompletedAt
) : DomainEvent
{
    public Guid OrderItemId { get; } = orderItemId;
    public DateTime PartiallyCompletedAt { get; } = partiallyCompletedAt;
}
