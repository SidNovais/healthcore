using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CancelExam;

public class CancelExamCommand(
    Guid orderId,
    Guid itemId,
    DateTime canceledAt
) : CommandBase
{
    public Guid OrderId { get; } = orderId;
    public Guid ItemId { get; } = itemId;
    public DateTime CanceledAt { get; } = canceledAt;
}
