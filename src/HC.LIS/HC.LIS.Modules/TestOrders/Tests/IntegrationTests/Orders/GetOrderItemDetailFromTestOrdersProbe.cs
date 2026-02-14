using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderItemDetails;

namespace HC.LIS.Modules.TestOrders.IntegrationTests.Orders;

public class GetOrderItemDetailFromTestOrdersProbe(
    Guid expectedOrderItemId,
    ITestOrdersModule testOrdersModule
) : IProbe<OrderItemDetailsDto>
{
    private ITestOrdersModule _testOrdersModule = testOrdersModule;
    private Guid _expectedOrderItemId = expectedOrderItemId;
    public string DescribeFailureTo() => "Order item it's not created";

    public async Task<OrderItemDetailsDto?> GetSampleAsync()
    {
        return await _testOrdersModule.ExecuteQueryAsync(new GetOrderItemDetailsQuery(_expectedOrderItemId)).ConfigureAwait(false);
    }

    public bool IsSatisfied(OrderItemDetailsDto? sample)
    {
        return sample != null;
    }
}
