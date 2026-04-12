using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.PartiallyCompleteExam;

namespace HC.LIS.API.Modules.TestOrders.Orders.PartiallyCompleteExam;

internal static class PartiallyCompleteExamEndpoint
{
    internal static async Task<IResult> Handle(
        Guid orderId,
        Guid itemId,
        ITestOrdersModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(
            new PartiallyCompleteExamCommand(orderId, itemId, SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
