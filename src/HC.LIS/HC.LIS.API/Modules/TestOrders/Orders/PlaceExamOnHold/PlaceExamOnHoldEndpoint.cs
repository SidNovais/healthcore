using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamOnHold;

namespace HC.LIS.API.Modules.TestOrders.Orders.PlaceExamOnHold;

internal static class PlaceExamOnHoldEndpoint
{
    internal static async Task<IResult> Handle(
        Guid orderId,
        Guid itemId,
        PlaceExamOnHoldRequest request,
        ITestOrdersModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(
            new PlaceExamOnHoldCommand(orderId, itemId, request.Reason, SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
