using System.Threading.Tasks;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.CreateOrder;
using HC.LIS.Modules.TestOrders.IntegrationTests.Physicians;

namespace HC.LIS.Modules.TestOrders.IntegrationTests.Orders;

internal static class OrderFactory
{
    public static async Task CreateAsync(
      ITestOrdersModule testOrdersModule
    )
    {
        await RegisterRequestingPhysicianAsync(testOrdersModule).ConfigureAwait(false);

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

    public static async Task RegisterRequestingPhysicianAsync(
      ITestOrdersModule testOrdersModule
    )
    {
        await PhysicianFactory.CreateAsync(
            testOrdersModule,
            OrderSampleData.RequestedBy,
            OrderSampleData.RequestedByFullName,
            licenceNumber: null
        ).ConfigureAwait(false);

        await TestBase.GetEventually(
            new GetPhysicianDetailsFromTestOrdersProbe(
                OrderSampleData.RequestedBy,
                testOrdersModule),
            15000
        ).ConfigureAwait(false);
    }
}
