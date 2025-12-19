using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Events;

public class OrderItemRequestedDomainEvent(
  Guid orderItemId,
  string specimenMnemonic,
  string materialType,
  string containerType,
  DateTime requestedAt
) : DomainEvent
{
    public Guid OrderItemId { get; } = orderItemId;
    public string SpecimenMnemonic { get; } = specimenMnemonic;
    public string MaterialType { get; } = materialType;
    public string ContainerType { get; } = containerType;
    public DateTime RequestedAt { get; } = requestedAt;
}
