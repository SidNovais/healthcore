using System;
using HC.Core.Infrastructure.EventBus;

namespace HC.LIS.Modules.TestOrders.IntegrationEvents;

public class OrderCreatedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid orderId,
    Guid patientId,
    Guid requestedBy,
    string orderPriority,
    DateTime requestedAt,
    string? patientName = null
) : IntegrationEvent(id, occurredAt)
{
    public Guid OrderId { get; } = orderId;
    public Guid PatientId { get; } = patientId;
    public Guid RequestedBy { get; } = requestedBy;
    public string OrderPriority { get; } = orderPriority;
    public DateTime RequestedAt { get; } = requestedAt;
    public string? PatientName { get; } = patientName;
}
