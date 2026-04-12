using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.RejectExam;

namespace HC.LIS.API.Modules.TestOrders.Orders.RejectExam;

internal static class RejectExamEndpoint
{
    internal static async Task<IResult> Handle(
        Guid orderId,
        Guid itemId,
        RejectExamRequest request,
        ITestOrdersModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(
            new RejectExamCommand(orderId, itemId, request.Reason, SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
