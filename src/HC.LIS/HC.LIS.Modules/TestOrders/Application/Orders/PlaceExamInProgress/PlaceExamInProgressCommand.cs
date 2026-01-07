using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamInProgress;

public class PlaceExamInProgressCommand(
    Guid orderId,
    Guid itemId,
    DateTime placedAt
) : CommandBase
{
    public Guid OrderId { get; } = orderId;
    public Guid ItemId { get; } = itemId;
    public DateTime PlacedAt { get; } = placedAt;
}
