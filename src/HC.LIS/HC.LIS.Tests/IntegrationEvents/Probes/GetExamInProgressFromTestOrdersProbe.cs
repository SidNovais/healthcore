using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderItemDetails;

namespace HC.LIS.Tests.IntegrationEvents.Probes;

public sealed class GetExamInProgressFromTestOrdersProbe(
    Guid orderItemId,
    ITestOrdersModule module,
    Func<OrderItemDetailsDto?, bool>? satisfiedWhen = null
) : IProbe<OrderItemDetailsDto>
{
    private readonly Func<OrderItemDetailsDto?, bool> _satisfiedWhen =
        satisfiedWhen ?? (dto => dto?.Status == "InProgress");

    public string DescribeFailureTo() =>
        $"OrderItem {orderItemId} did not reach InProgress status";

    public async Task<OrderItemDetailsDto?> GetSampleAsync() =>
        await module
            .ExecuteQueryAsync(new GetOrderItemDetailsQuery(orderItemId))
            .ConfigureAwait(false);

    public bool IsSatisfied(OrderItemDetailsDto? sample) => _satisfiedWhen(sample);
}
