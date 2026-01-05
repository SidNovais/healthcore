using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Orders.RejectExam;

public class RejectExamCommand(
    Guid orderId,
    Guid itemId,
    string reason,
    DateTime rejectedAt
) : CommandBase
{
    public Guid OrderId { get; } = orderId;
    public Guid ItemId { get; } = itemId;
    public string Reason { get; } = reason;
    public DateTime RejectedAt { get; } = rejectedAt;
}
