using System;
using System.Threading;
using System.Threading.Tasks;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.TestOrders.Application.Orders.CancelExam;
using HC.LIS.Modules.TestOrders.Application.Orders.CompleteExam;
using HC.LIS.Modules.TestOrders.Application.Orders.PartiallyCompleteExam;
using HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamInProgress;
using HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamOnHold;
using HC.LIS.Modules.TestOrders.Application.Orders.RejectExam;
using HC.LIS.Modules.TestOrders.Domain.Orders.Events;
using HC.LIS.Modules.TestOrders.IntegrationEvents;
using NSubstitute;

namespace HC.LIS.Modules.TestOrders.UnitTests.Orders;

public sealed class OrdersPublishEventHandlersTests
{
    private readonly IEventsBus _bus = Substitute.For<IEventsBus>();
    private readonly Guid _itemId = Guid.NewGuid();

    [Fact]
    public async Task CanceledHandlerPublishesIntegrationEventWithOrderItemId()
    {
        var sut = new ExamCanceledPublishEventNotificationHandler(_bus);

        await sut.Handle(
            new ExamCanceledNotification(new OrderItemCanceledDomainEvent(_itemId, DateTime.UtcNow), Guid.NewGuid()),
            CancellationToken.None).ConfigureAwait(true);

        await _bus.Received(1).Publish(Arg.Is<OrderItemCanceledIntegrationEvent>(e => e.OrderItemId == _itemId))
            .ConfigureAwait(true);
    }

    [Fact]
    public async Task RejectedHandlerPublishesIntegrationEventWithReason()
    {
        var sut = new ExamRejectedPublishEventNotificationHandler(_bus);

        await sut.Handle(
            new ExamRejectedNotification(new OrderItemRejectedDomainEvent(_itemId, "hemolyzed", DateTime.UtcNow), Guid.NewGuid()),
            CancellationToken.None).ConfigureAwait(true);

        await _bus.Received(1).Publish(Arg.Is<OrderItemRejectedIntegrationEvent>(
            e => e.OrderItemId == _itemId && e.Reason == "hemolyzed")).ConfigureAwait(true);
    }

    [Fact]
    public async Task PlacedOnHoldHandlerPublishesIntegrationEventWithReason()
    {
        var sut = new ExamPlacedOnHoldPublishEventNotificationHandler(_bus);

        await sut.Handle(
            new ExamPlacedOnHoldNotification(new OrderItemPlacedOnHoldDomainEvent(_itemId, "awaiting reagent", DateTime.UtcNow), Guid.NewGuid()),
            CancellationToken.None).ConfigureAwait(true);

        await _bus.Received(1).Publish(Arg.Is<OrderItemPlacedOnHoldIntegrationEvent>(
            e => e.OrderItemId == _itemId && e.Reason == "awaiting reagent")).ConfigureAwait(true);
    }

    [Fact]
    public async Task PlacedInProgressHandlerPublishesIntegrationEventWithOrderItemId()
    {
        var sut = new ExamPlacedInProgressPublishEventNotificationHandler(_bus);

        await sut.Handle(
            new ExamPlacedInProgressNotification(new OrderItemPlacedInProgressDomainEvent(_itemId, DateTime.UtcNow), Guid.NewGuid()),
            CancellationToken.None).ConfigureAwait(true);

        await _bus.Received(1).Publish(Arg.Is<OrderItemPlacedInProgressIntegrationEvent>(e => e.OrderItemId == _itemId))
            .ConfigureAwait(true);
    }

    [Fact]
    public async Task CompletedHandlerPublishesIntegrationEventWithOrderItemId()
    {
        var sut = new ExamCompletedPublishEventNotificationHandler(_bus);

        await sut.Handle(
            new ExamCompletedNotification(new OrderItemCompletedDomainEvent(_itemId, DateTime.UtcNow), Guid.NewGuid()),
            CancellationToken.None).ConfigureAwait(true);

        await _bus.Received(1).Publish(Arg.Is<OrderItemCompletedIntegrationEvent>(e => e.OrderItemId == _itemId))
            .ConfigureAwait(true);
    }

    [Fact]
    public async Task PartiallyCompletedHandlerPublishesIntegrationEventWithOrderItemId()
    {
        var sut = new ExamPartiallyCompletedPublishEventNotificationHandler(_bus);

        await sut.Handle(
            new ExamPartiallyCompletedNotification(new OrderItemPartiallyCompletedDomainEvent(_itemId, DateTime.UtcNow), Guid.NewGuid()),
            CancellationToken.None).ConfigureAwait(true);

        await _bus.Received(1).Publish(Arg.Is<OrderItemPartiallyCompletedIntegrationEvent>(e => e.OrderItemId == _itemId))
            .ConfigureAwait(true);
    }
}
