using System;
using HC.COre.Infrastructure.EventBus;

namespace HC.LIS.Modules.TestOrders.IntegrationEvents;

public class OrderItemRequestedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid orderItemId,
    Guid orderId,
    string specimenMnemonic,
    string materialType,
    string containerType,
    string additive,
    string processingType,
    string storageCondition,
    DateTime requestedAt
) : IntegrationEvent(id, occurredAt)
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
