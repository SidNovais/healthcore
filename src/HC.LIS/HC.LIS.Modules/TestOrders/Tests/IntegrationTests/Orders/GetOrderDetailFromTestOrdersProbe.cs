using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;

namespace HC.LIS.Modules.TestOrders.IntegrationTests.Orders;

public class GetOrderDetailFromTestOrdersProbe(
    Guid expectedOrderId,
    ITestOrdersModule testOrdersModule
) : IProbe<OrderDetailsDto>
{
    private ITestOrdersModule _testOrdersModule = testOrdersModule;
    private Guid _expectedOrderId = expectedOrderId;
    public string DescribeFailureTo() => "Order it's not created";

    public async Task<OrderDetailsDto?> GetSampleAsync()
    {
        return await _testOrdersModule.ExecuteQueryAsync(new GetOrderDetailsQuery(_expectedOrderId)).ConfigureAwait(false);
    }

    public bool IsSatisfied(OrderDetailsDto? sample)
    {
        return sample != null;
    }
}
