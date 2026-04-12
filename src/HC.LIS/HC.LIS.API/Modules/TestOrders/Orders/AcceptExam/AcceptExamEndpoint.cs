using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.AcceptExam;

namespace HC.LIS.API.Modules.TestOrders.Orders.AcceptExam;

internal static class AcceptExamEndpoint
{
    internal static async Task<IResult> Handle(
        Guid orderId,
        Guid itemId,
        ITestOrdersModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(
            new AcceptExamCommand(orderId, itemId, SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
