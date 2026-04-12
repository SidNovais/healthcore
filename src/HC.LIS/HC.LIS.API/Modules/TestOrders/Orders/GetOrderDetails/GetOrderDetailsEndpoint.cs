using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;

namespace HC.LIS.API.Modules.TestOrders.Orders.GetOrderDetails;

internal static class GetOrderDetailsEndpoint
{
    internal static async Task<IResult> Handle(
        Guid orderId,
        ITestOrdersModule module,
        CancellationToken ct)
    {
        var result = await module.ExecuteQueryAsync(
            new GetOrderDetailsQuery(orderId)).ConfigureAwait(false);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}
