using System;
using System.Text.Json.Serialization;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CompleteExam;

[method: JsonConstructor]
public class CompleteExamByExamIdCommand(
    Guid id,
    Guid orderId,
    Guid orderItemId,
    DateTime completedAt
) : InternalCommandBase(id)
{
    public Guid OrderId { get; } = orderId;
    public Guid OrderItemId { get; } = orderItemId;
    public DateTime CompletedAt { get; } = completedAt;
}
