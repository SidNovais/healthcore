using System;
using HC.Core.Infrastructure.EventBus;

namespace HC.LIS.Modules.TestOrders.IntegrationEvents;

public class OrderItemAcceptedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid orderItemId,
    Guid orderId,
    Guid patientId,
    string examMnemonic,
    string containerType
) : IntegrationEvent(id, occurredAt)
{
    public Guid OrderItemId { get; } = orderItemId;
    public Guid OrderId { get; } = orderId;
    public Guid PatientId { get; } = patientId;
    public string ExamMnemonic { get; } = examMnemonic;
    public string ContainerType { get; } = containerType;
}
