using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Events;

public class OrderCreatedDomainEvent(
  Guid orderId,
  Guid patientId,
  Guid requestedBy,
  string orderPriority,
  DateTime requestedAt
) : DomainEvent
{
    public Guid OrderId { get; } = orderId;
    public Guid PatientId { get; } = patientId;
    public Guid RequestedBy { get; } = requestedBy;
    public string OrderPriority { get; } = orderPriority;
    public DateTime RequestedAt { get; } = requestedAt;
}
