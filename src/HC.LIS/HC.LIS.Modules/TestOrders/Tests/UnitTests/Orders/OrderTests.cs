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
        _sut.CancelExam(new OrderItemId(OrderSampleData.OrderItemId), canceledAt);
        OrderItemCanceledDomainEvent orderItemCanceledDomainEvent = AssertPublishedDomainEvent<OrderItemCanceledDomainEvent>(_sut);
        orderItemCanceledDomainEvent.OrderItemId.Should().Be(OrderSampleData.OrderItemId);
        orderItemCanceledDomainEvent.CanceledAt.Should().Be(canceledAt);
    }

    [Fact]
    public void RejectExamIsSuccessful()
    {
        DateTime rejectedAt = SystemClock.Now;
        string reason = "Test";
        _sut.RejectExam(
            new OrderItemId(OrderSampleData.OrderItemId),
            reason,
            rejectedAt
        );
        OrderItemRejectedDomainEvent orderItemRejectedDomainEvent = AssertPublishedDomainEvent<OrderItemRejectedDomainEvent>(_sut);
        orderItemRejectedDomainEvent.OrderItemId.Should().Be(OrderSampleData.OrderItemId);
        orderItemRejectedDomainEvent.Reason.Should().Be(reason);
        orderItemRejectedDomainEvent.RejectedAt.Should().Be(rejectedAt);
    }

    [Fact]
    public void PlaceExamOnHoldIsSuccessful()
    {
        DateTime placeOnHoldAt = SystemClock.Now;
        string reason = "Test";
        _sut.PlaceExamOnHold(
            new OrderItemId(OrderSampleData.OrderItemId),
            reason,
            placeOnHoldAt
        );
        OrderItemPlacedOnHoldDomainEvent orderItemPlacedOnHoldDomainEvent = AssertPublishedDomainEvent<OrderItemPlacedOnHoldDomainEvent>(_sut);
        orderItemPlacedOnHoldDomainEvent.OrderItemId.Should().Be(OrderSampleData.OrderItemId);
        orderItemPlacedOnHoldDomainEvent.Reason.Should().Be(reason);
        orderItemPlacedOnHoldDomainEvent.PlaceOnHoldAt.Should().Be(placeOnHoldAt);
    }

    [Fact]
    public void AcceptExamIsSuccessful()
    {
        DateTime acceptedAt = SystemClock.Now;
        _sut.AcceptExam(
            new OrderItemId(OrderSampleData.OrderItemId),
            acceptedAt
        );
        OrderItemAcceptedDomainEvent orderItemAcceptedDomainEvent = AssertPublishedDomainEvent<OrderItemAcceptedDomainEvent>(_sut);
        orderItemAcceptedDomainEvent.OrderItemId.Should().Be(OrderSampleData.OrderItemId);
        orderItemAcceptedDomainEvent.AcceptedAt.Should().Be(acceptedAt);
    }

    [Fact]
    public void PlaceExamInProgressIsSuccessful()
    {
        DateTime placeInProgressAt = SystemClock.Now;
        _sut.PlaceExamInProgress(
            new OrderItemId(OrderSampleData.OrderItemId),
            placeInProgressAt
        );
        OrderItemPlacedInProgressDomainEvent orderItemPlacedInProgressDomainEvent = AssertPublishedDomainEvent<OrderItemPlacedInProgressDomainEvent>(_sut);
        orderItemPlacedInProgressDomainEvent.OrderItemId.Should().Be(OrderSampleData.OrderItemId);
        orderItemPlacedInProgressDomainEvent.PlaceInProgressAt.Should().Be(placeInProgressAt);
    }

}
