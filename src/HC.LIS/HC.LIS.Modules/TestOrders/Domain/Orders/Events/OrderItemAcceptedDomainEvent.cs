using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Events;

public class OrderItemAcceptedDomainEvent(
  Guid orderItemId,
  Guid orderId,
  Guid patientId,
  string examMnemonic,
  string containerType,
  bool isUrgent,
  DateTime acceptedAt
) : DomainEvent
{
    public Guid OrderItemId { get; } = orderItemId;
    public Guid OrderId { get; } = orderId;
    public Guid PatientId { get; } = patientId;
    public string ExamMnemonic { get; } = examMnemonic;
    public string ContainerType { get; } = containerType;
    public bool IsUrgent { get; } = isUrgent;
    public DateTime AcceptedAt { get; } = acceptedAt;
}
