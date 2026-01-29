using System.Threading.Tasks;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.CreateOrder;

namespace HC.LIS.Modules.TestOrders.IntegrationTests.Orders;

internal static class OrderFactory
{
    public static async Task CreateAsync(
      ITestOrdersModule testOrdersModule
    )
    {
        await testOrdersModule.ExecuteCommandAsync(
          new CreateOrderCommand(
            OrderSampleData.OrderId,
            OrderSampleData.PatientId,
            OrderSampleData.RequestedBy,
            OrderSampleData.OrderPriority,
            OrderSampleData.RequestedAt
          )
        ).ConfigureAwait(false);
    }
}
