using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Events;

public class OrderItemRequestedDomainEvent(
  Guid orderItemId,
  string specimenType,
  DateTime requestedAt
) : DomainEvent
{
    public Guid OrderItemId { get; } = orderItemId;
    public string SpecimenType { get; } = specimenType;
    public DateTime RequestedAt { get; } = requestedAt;
}
