using System;
using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Domain.Orders.Events;
using HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

namespace HC.LIS.Modules.TestOrders.Domain.Orders;

public class OrderItem : Entity
{
    internal OrderItemId OrderItemId { get; private set; }
    internal SpecimenRequirement _speciamentRequirement;
    internal OrderItemStatus _status;
    internal DateTime _requestedAt;
    internal DateTime _canceledAt;
    internal DateTime _onHoldAt;
    internal DateTime _acceptedAt;
    internal DateTime _inProgressAt;
    internal DateTime _partiallyCompletedAt;
    internal DateTime _completedAt;

    public static OrderItem Request(
        OrderItemRequestedDomainEvent domainEvent
    )
    {
        OrderItem order = new();
        order.Apply(domainEvent);
        return order;
    }

    public void Cancel(OrderItemCanceledDomainEvent domainEvent)
    {
        CheckRule(new CannotCancelOrderItemThanMoreOnceRule(_status));
        CheckRule(new CannotCancelOrderItemWhenIsRejectedRule(_status));
        Apply(domainEvent);
    }
    public void PlaceOnHold(OrderItemPlacedOnHoldDomainEvent domainEvent)
        => Apply(domainEvent);
    public void Accept(OrderItemAcceptedDomainEvent domainEvent)
        => Apply(domainEvent);
    public void PlaceInProgress(OrderItemPlacedInProgressDomainEvent domainEvent)
        => Apply(domainEvent);
    public void PartiallyComplete(OrderItemPartiallyCompletedDomainEvent domainEvent)
        => Apply(domainEvent);
    public void Complete(OrderItemCompletedDomainEvent domainEvent)
        => Apply(domainEvent);
    public void Reject(OrderItemRejectedDomainEvent domainEvent)
        => Apply(domainEvent);

    private void Apply(IDomainEvent domainEvent) => When((dynamic)domainEvent);

    private void When(OrderItemRequestedDomainEvent domainEvent)
    {
        OrderItemId = new(domainEvent.OrderItemId);
        _speciamentRequirement = SpecimenRequirement.Of(
            domainEvent.SpecimenMnemonic,
            domainEvent.MaterialType,
            domainEvent.ContainerType,
            domainEvent.Additive,
            domainEvent.ProcessingType,
            domainEvent.StorageCondition
        );
        _status = OrderItemStatus.Requested;
        _requestedAt = domainEvent.RequestedAt;
    }

    private void When(OrderItemCanceledDomainEvent domainEvent)
    {
        _status = OrderItemStatus.Canceled;
        _canceledAt = domainEvent.CanceledAt;
    }

    private void When(OrderItemPlacedOnHoldDomainEvent domainEvent)
    {
        _status = OrderItemStatus.OnHold;
        _onHoldAt = domainEvent.PlaceOnHoldAt;
    }

    private void When(OrderItemAcceptedDomainEvent domainEvent)
    {
        _status = OrderItemStatus.Accepted;
        _acceptedAt = domainEvent.AcceptedAt;
    }

    private void When(OrderItemPlacedInProgressDomainEvent domainEvent)
    {
        _status = OrderItemStatus.InProgress;
        _inProgressAt = domainEvent.PlaceInProgressAt;
    }

    private void When(OrderItemPartiallyCompletedDomainEvent domainEvent)
    {
        _status = OrderItemStatus.PartiallyCompleted;
        _partiallyCompletedAt = domainEvent.PartiallyCompletedAt;
    }

    private void When(OrderItemCompletedDomainEvent domainEvent)
    {
        _status = OrderItemStatus.Completed;
        _completedAt = domainEvent.CompletedAt;
    }

    private void When(OrderItemRejectedDomainEvent domainEvent)
    {
        _status = OrderItemStatus.Rejected;
        _completedAt = domainEvent.RejectedAt;
    }
}
