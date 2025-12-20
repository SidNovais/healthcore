using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Events;

public class OrderItemAcceptedDomainEvent(
  Guid orderItemId,
  DateTime acceptedAt
) : DomainEvent
{
    public Guid OrderItemId { get; } = orderItemId;
    public DateTime AcceptedAt { get; } = acceptedAt;
}
