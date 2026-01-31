using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;

public class GetOrderDetailsQuery(
  Guid orderId
) : QueryBase<OrderDetailsDto?>
{
    public Guid OrderId { get; } = orderId;
}
