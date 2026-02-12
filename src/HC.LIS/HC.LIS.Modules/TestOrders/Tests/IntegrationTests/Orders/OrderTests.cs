using System;
using FluentAssertions;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;

namespace HC.LIS.Modules.TestOrders.IntegrationTests.Orders;

public class OrderTests : TestBase
{
    public OrderTests() : base(Guid.CreateVersion7())
    {
        OrderFactory.CreateAsync(TestOrdersModule).GetAwaiter().GetResult();
    }

    [Fact]
    public async void CreateOrderIsSuccessfully()
    {
        OrderDetailsDto? orderDetail = await GetEventually(
            new GetOrderDetailFromTestOrdersProbe(
                OrderSampleData.OrderId,
                TestOrdersModule
            ),
            15000
        ).ConfigureAwait(true);
        orderDetail?.OrderId.Should().Be(OrderSampleData.OrderId);
        orderDetail?.PatientId.Should().Be(OrderSampleData.PatientId);
        orderDetail?.RequestedBy.Should().Be(OrderSampleData.RequestedBy);
        orderDetail?.OrderPriority.Should().Be(OrderSampleData.OrderPriority);
    }
}
