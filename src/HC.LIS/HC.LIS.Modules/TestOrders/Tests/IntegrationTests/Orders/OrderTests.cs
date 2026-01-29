using System;

namespace HC.LIS.Modules.TestOrders.IntegrationTests.Orders;

public class TestOrders : TestBase
{
    public TestOrders() : base(Guid.CreateVersion7())
    {
        OrderFactory.CreateAsync(TestOrdersModule).GetAwaiter().GetResult();
    }
}
