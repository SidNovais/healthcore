using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Events;

public class OrderItemRejectedDomainEvent(
  Guid orderItemId,
  string reason,
  DateTime rejectedAt
) : DomainEvent
{
    public Guid OrderItemId { get; } = orderItemId;
    public string Reason { get; } = reason;
    public DateTime RejectedAt { get; } = rejectedAt;
}
