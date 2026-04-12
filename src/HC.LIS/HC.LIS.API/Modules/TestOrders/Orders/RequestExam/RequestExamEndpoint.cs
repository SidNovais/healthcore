using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.RequestExam;

namespace HC.LIS.API.Modules.TestOrders.Orders.RequestExam;

internal static class RequestExamEndpoint
{
    internal static async Task<IResult> Handle(
        Guid orderId,
        RequestExamRequest request,
        ITestOrdersModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(new RequestExamCommand(
            orderId,
            request.ItemId,
            request.ExamMnemonic,
            request.SpecimenMnemonic,
            request.MaterialType,
            request.ContainerType,
            request.Additive,
            request.ProcessingType,
            request.StorageCondition,
            SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
