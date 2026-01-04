using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Orders.AcceptExam;

public class AcceptExamCommand(
    Guid orderId,
    Guid itemId,
    DateTime acceptedAt
) : CommandBase
{
    public Guid OrderId { get; } = orderId;
    public Guid ItemId { get; } = itemId;
    public DateTime AcceptedAt { get; } = acceptedAt;
}
