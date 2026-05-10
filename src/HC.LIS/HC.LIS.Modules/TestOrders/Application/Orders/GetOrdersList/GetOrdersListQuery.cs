using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Orders.GetOrdersList;

public class GetOrdersListQuery() : QueryBase<IReadOnlyCollection<OrderListItemDto>>;
