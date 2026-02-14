using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Orders.GetOrderItemDetails;

public class GetOrderItemDetailsQuery(
  Guid orderItemId
) : QueryBase<OrderItemDetailsDto?>
{
    public Guid OrderItemId { get; } = orderItemId;
}
