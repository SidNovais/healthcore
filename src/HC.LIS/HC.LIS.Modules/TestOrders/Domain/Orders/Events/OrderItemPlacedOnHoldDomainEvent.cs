using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Events;

public class OrderItemPlacedOnHoldDomainEvent(
  Guid orderItemId,
  string reason,
  DateTime placeOnHoldAt
) : DomainEvent
{
    public Guid OrderItemId { get; } = orderItemId;
    public string Reason { get; } = reason;
    public DateTime PlaceOnHoldAt { get; } = placeOnHoldAt;
}
