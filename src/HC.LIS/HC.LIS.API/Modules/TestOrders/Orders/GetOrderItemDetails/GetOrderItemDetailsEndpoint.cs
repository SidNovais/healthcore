using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderItemDetails;

namespace HC.LIS.API.Modules.TestOrders.Orders.GetOrderItemDetails;

internal static class GetOrderItemDetailsEndpoint
{
    internal static async Task<IResult> Handle(
        Guid itemId,
        ITestOrdersModule module,
        CancellationToken ct)
    {
        var result = await module.ExecuteQueryAsync(
            new GetOrderItemDetailsQuery(itemId)).ConfigureAwait(false);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}
