using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrdersList;

namespace HC.LIS.API.Modules.TestOrders.Orders.GetOrderList;

internal static class GetOrderListEndpoint
{
    internal static async Task<IResult> Handle(
        ITestOrdersModule module,
        CancellationToken ct)
    {
        IReadOnlyCollection<OrderListItemDto> result = await module.ExecuteQueryAsync(
            new GetOrdersListQuery()).ConfigureAwait(false);

        return TypedResults.Ok(result);
    }
}
