using FluentAssertions;
using HC.Core.UnitTests;
using HC.LIS.Modules.TestOrders.Domain.Orders;
using HC.LIS.Modules.TestOrders.Domain.Orders.Events;
using HC.LIS.Modules.TestOrders.UnitTests.Orders;

namespace HC.Lis.Modules.TestOrders.UnitTests.Orders;

public class OrderTests : TestBase
{
    readonly Order _sut;

    public OrderTests()
    {
        _sut = OrderFactory.Create();
    }

    [Fact]
    public void CreateOrderIsSuccessful()
    {
        OrderCreatedDomainEvent orderCreatedDomainEvent = AssertPublishedDomainEvent<OrderCreatedDomainEvent>(_sut);
        orderCreatedDomainEvent.OrderId.Should().Be(OrderSampleData.OrderId);
        orderCreatedDomainEvent.PatientId.Should().Be(OrderSampleData.PatientId);
        orderCreatedDomainEvent.RequestedBy.Should().Be(OrderSampleData.RequestedBy);
        orderCreatedDomainEvent.OrderPriority.Should().Be(OrderSampleData.OrderPriority);
        orderCreatedDomainEvent.RequestedAt.Should().Be(OrderSampleData.RequestedAt);
    }
}
