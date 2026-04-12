using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.CancelExam;

namespace HC.LIS.API.Modules.TestOrders.Orders.CancelExam;

internal static class CancelExamEndpoint
{
    internal static async Task<IResult> Handle(
        Guid orderId,
        Guid itemId,
        ITestOrdersModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(
            new CancelExamCommand(orderId, itemId, SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
