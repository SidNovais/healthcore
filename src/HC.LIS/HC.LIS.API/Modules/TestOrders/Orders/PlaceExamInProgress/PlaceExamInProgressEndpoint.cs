using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamInProgress;

namespace HC.LIS.API.Modules.TestOrders.Orders.PlaceExamInProgress;

internal static class PlaceExamInProgressEndpoint
{
    internal static async Task<IResult> Handle(
        Guid orderId,
        Guid itemId,
        ITestOrdersModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(
            new PlaceExamInProgressCommand(orderId, itemId, SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
