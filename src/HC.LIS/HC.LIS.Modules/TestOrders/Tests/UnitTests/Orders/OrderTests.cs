using System;
using FluentAssertions;
using HC.Core.Domain;
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

    [Fact]
    public void RequestExamIsSuccessful()
    {
        _sut.RequestExam(
            OrderSampleData.OrderItemId,
            SpecimenRequirement.Of(
                OrderSampleData.SpecimenMnemonic,
                OrderSampleData.MaterialType,
                OrderSampleData.ContainerType,
                OrderSampleData.Additive,
                OrderSampleData.ProcessingType,
                OrderSampleData.StorageCondition
            ),
            OrderSampleData.RequestedAt
        );
        OrderItemRequestedDomainEvent orderItemRequestedDomainEvent = AssertPublishedDomainEvent<OrderItemRequestedDomainEvent>(_sut);
        orderItemRequestedDomainEvent.OrderItemId.Should().Be(OrderSampleData.OrderItemId);
        orderItemRequestedDomainEvent.SpecimenMnemonic.Should().Be(OrderSampleData.SpecimenMnemonic);
        orderItemRequestedDomainEvent.MaterialType.Should().Be(OrderSampleData.MaterialType);
        orderItemRequestedDomainEvent.ContainerType.Should().Be(OrderSampleData.ContainerType);
        orderItemRequestedDomainEvent.Additive.Should().Be(OrderSampleData.Additive);
        orderItemRequestedDomainEvent.ProcessingType.Should().Be(OrderSampleData.ProcessingType);
        orderItemRequestedDomainEvent.StorageCondition.Should().Be(OrderSampleData.StorageCondition);
        orderItemRequestedDomainEvent.RequestedAt.Should().Be(OrderSampleData.RequestedAt);
    }

    [Fact]
    public void CancelExamIsSuccessful()
    {
        DateTime canceledAt = SystemClock.Now;
        _sut.RequestExam(
            OrderSampleData.OrderItemId,
            SpecimenRequirement.Of(
                OrderSampleData.SpecimenMnemonic,
                OrderSampleData.MaterialType,
                OrderSampleData.ContainerType,
                OrderSampleData.Additive,
                OrderSampleData.ProcessingType,
                OrderSampleData.StorageCondition
            ),
            OrderSampleData.RequestedAt
        );
        _sut.CancelExam(new OrderItemId(OrderSampleData.OrderItemId), canceledAt);
        OrderItemCanceledDomainEvent orderItemRequestedDomainEvent = AssertPublishedDomainEvent<OrderItemCanceledDomainEvent>(_sut);
        orderItemRequestedDomainEvent.OrderItemId.Should().Be(OrderSampleData.OrderItemId);
        orderItemRequestedDomainEvent.CanceledAt.Should().Be(canceledAt);
    }
}
