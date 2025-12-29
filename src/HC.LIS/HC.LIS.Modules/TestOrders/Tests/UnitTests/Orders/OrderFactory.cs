using HC.LIS.Modules.TestOrders.Domain.Orders;
using HC.LIS.Modules.TestOrders.Domain.Patients;
using HC.LIS.Modules.TestOrders.Domain.Physicians;
using HC.LIS.Modules.TestOrders.UnitTests.Orders;

namespace HC.Lis.Modules.TestOrders.UnitTests.Orders;

internal static class OrderFactory
{
    public static Order Create()
    {
        Order order = Order.Create(
            OrderSampleData.OrderId,
            new PatientId(OrderSampleData.PatientId),
            new PhysicianId(OrderSampleData.RequestedBy),
            OrderPriority.Of(OrderSampleData.OrderPriority),
            OrderSampleData.RequestedAt
        );
        return order;
    }
}
