using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderItemDetails;

namespace HC.LIS.Tests.IntegrationEvents.Probes;

public sealed class GetExamCompletedFromTestOrdersProbe(
    Guid orderItemId,
    ITestOrdersModule module
) : IProbe<OrderItemDetailsDto>
{
    public string DescribeFailureTo() =>
        $"OrderItem {orderItemId} did not reach Completed status";

    public async Task<OrderItemDetailsDto?> GetSampleAsync() =>
        await module
            .ExecuteQueryAsync(new GetOrderItemDetailsQuery(orderItemId))
            .ConfigureAwait(false);

    public bool IsSatisfied(OrderItemDetailsDto? sample) =>
        sample?.Status == "Completed";
}
