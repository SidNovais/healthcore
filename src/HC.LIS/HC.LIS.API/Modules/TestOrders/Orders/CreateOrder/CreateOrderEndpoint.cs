using HC.Core.Domain;
using HC.LIS.API.Common;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.CreateOrder;

namespace HC.LIS.API.Modules.TestOrders.Orders.CreateOrder;

internal static class CreateOrderEndpoint
{
    internal static async Task<IResult> Handle(
        CreateOrderRequest request,
        ITestOrdersModule module,
        CancellationToken ct)
    {
        var id = await module.ExecuteCommandAsync(new CreateOrderCommand(
            request.OrderId,
            request.PatientId,
            request.RequestedBy,
            request.OrderPriority,
            SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.Created($"/api/v1/orders/{id}", new CreatedIdResponse(id));
    }
}
