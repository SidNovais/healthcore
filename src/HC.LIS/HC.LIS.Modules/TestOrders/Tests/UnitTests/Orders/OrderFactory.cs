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
            ActivePhysician(),
            OrderPriority.Of(OrderSampleData.OrderPriority),
            OrderSampleData.RequestedAt
        );
        return order;
    }

    public static RequestingPhysician ActivePhysician() => RequestingPhysician.Of(
        new PhysicianId(OrderSampleData.RequestedBy),
        OrderSampleData.RequestedByFullName,
        isActive: true
    );

    public static RequestingPhysician InactivePhysician() => RequestingPhysician.Of(
        new PhysicianId(OrderSampleData.RequestedBy),
        OrderSampleData.RequestedByFullName,
        isActive: false
    );
}
