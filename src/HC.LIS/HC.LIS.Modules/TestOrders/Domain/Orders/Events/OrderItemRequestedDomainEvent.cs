using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Events;

public class OrderItemRequestedDomainEvent(
  Guid orderItemId,
  Guid orderId,
  string specimenMnemonic,
  string materialType,
  string containerType,
  string additive,
  string processingType,
  string storageCondition,
  DateTime requestedAt
) : DomainEvent
{
    public Guid OrderItemId { get; } = orderItemId;
    public Guid OrderId { get; } = orderId;
    public string SpecimenMnemonic { get; } = specimenMnemonic;
    public string MaterialType { get; } = materialType;
    public string ContainerType { get; } = containerType;
    public string Additive { get; } = additive;
    public string ProcessingType { get; } = processingType;
    public string StorageCondition { get; } = storageCondition;
    public DateTime RequestedAt { get; } = requestedAt;
}
