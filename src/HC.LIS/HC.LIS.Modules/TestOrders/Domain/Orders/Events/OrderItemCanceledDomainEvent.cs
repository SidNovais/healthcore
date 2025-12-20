using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Events;

public class OrderItemCanceledDomainEvent(
  Guid orderItemId,
  DateTime canceledAt
) : DomainEvent
{
    public Guid OrderItemId { get; } = orderItemId;
    public DateTime CanceledAt { get; } = canceledAt;
}
