using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CompleteExam;

public class CompleteExamCommand(
    Guid orderId,
    Guid itemId,
    DateTime completedAt
) : CommandBase
{
    public Guid OrderId { get; } = orderId;
    public Guid ItemId { get; } = itemId;
    public DateTime CompletedAt { get; } = completedAt;
}
