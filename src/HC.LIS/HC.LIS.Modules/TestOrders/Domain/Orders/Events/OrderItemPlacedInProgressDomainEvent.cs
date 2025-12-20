using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Events;

public class OrderItemPlacedInProgressDomainEvent(
  Guid orderItemId,
  DateTime placeInProgressAt
) : DomainEvent
{
    public Guid OrderItemId { get; } = orderItemId;
    public DateTime PlaceInProgressAt { get; } = placeInProgressAt;
}
