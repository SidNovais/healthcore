using System.Text.Json.Serialization;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamInProgress;

[method: JsonConstructor]
public class PlaceExamInProgressByExamIdCommand(
    Guid id,
    Guid orderId,
    Guid orderItemId,
    DateTime placedAt
) : InternalCommandBase(id)
{
    public Guid OrderId { get; } = orderId;
    public Guid OrderItemId { get; } = orderItemId;
    public DateTime PlacedAt { get; } = placedAt;
}
