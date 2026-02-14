using System;
using System.Collections.Generic;
using System.Linq;
using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.TestOrders.Domain.Orders.Events;
using HC.LIS.Modules.TestOrders.Domain.Patients;
using HC.LIS.Modules.TestOrders.Domain.Physicians;

namespace HC.LIS.Modules.TestOrders.Domain.Orders;

public class Order : AggregateRoot
{
    private PatientId _patientId;
    private PhysicianId _requestedBy;
    private OrderPriority _orderPriority;
    private IList<OrderItem> _items = [];
    private DateTime _requestedAt;
    private Order() { }
    protected override void Apply(IDomainEvent domainEvent) => When((dynamic)@domainEvent);
    public static Order Create(
        Guid id,
        PatientId patientId,
        PhysicianId requestedBy,
        OrderPriority orderPriority,
        DateTime requestedAt
    )
    {
        Order order = new();
        OrderCreatedDomainEvent orderCreatedDomainEvent = new(
            id,
            patientId.Value,
            requestedBy.Value,
            orderPriority.Value,
            requestedAt
        );
        order.Apply(orderCreatedDomainEvent);
        order.AddDomainEvent(orderCreatedDomainEvent);
        return order;
    }

    public void RequestExam(
        Guid orderItemId,
        SpecimenRequirement specimenRequirement,
        DateTime requestedAt
    )
    {
        OrderItemRequestedDomainEvent orderItemRequestedDomainEvent = new(
            orderItemId,
            Id,
            specimenRequirement.SpecimenMnemonic,
            specimenRequirement.MaterialType,
            specimenRequirement.ContainerType,
            specimenRequirement.Additive,
            specimenRequirement.ProcessingType,
            specimenRequirement.StorageCondition,
            requestedAt
        );
        Apply(orderItemRequestedDomainEvent);
        AddDomainEvent(orderItemRequestedDomainEvent);
    }

    public void CancelExam(OrderItemId orderItemId, DateTime canceledAt)
    {
        OrderItemCanceledDomainEvent orderItemCanceledDomainEvent = new(
            orderItemId.Value,
            canceledAt
        );
        Apply(orderItemCanceledDomainEvent);
        AddDomainEvent(orderItemCanceledDomainEvent);
    }

    public void PlaceExamOnHold(
        OrderItemId orderItemId,
        string reason,
        DateTime placedOnHoldAt
    )
    {
        OrderItemPlacedOnHoldDomainEvent orderItemPlacedOnHoldDomainEvent = new(
            orderItemId.Value,
            reason,
            placedOnHoldAt
        );
        Apply(orderItemPlacedOnHoldDomainEvent);
        AddDomainEvent(orderItemPlacedOnHoldDomainEvent);
    }

    public void AcceptExam(
        OrderItemId orderItemId,
        DateTime acceptedAt
    )
    {
        OrderItemAcceptedDomainEvent orderItemAcceptedDomainEvent = new(
            orderItemId.Value,
            acceptedAt
        );
        Apply(orderItemAcceptedDomainEvent);
        AddDomainEvent(orderItemAcceptedDomainEvent);
    }

    public void PlaceExamInProgress(
        OrderItemId orderItemId,
        DateTime placeInProgressAt
    )
    {
        OrderItemPlacedInProgressDomainEvent orderItemPlacedInProgressDomainEvent = new(
            orderItemId.Value,
            placeInProgressAt
        );
        Apply(orderItemPlacedInProgressDomainEvent);
        AddDomainEvent(orderItemPlacedInProgressDomainEvent);
    }

    public void PartiallyCompleteExam(
        OrderItemId orderItemId,
        DateTime partiallyCompletedAt
    )
    {
        OrderItemPartiallyCompletedDomainEvent orderItemPartiallyCompletedDomainEvent = new(
            orderItemId.Value,
            partiallyCompletedAt
        );
        Apply(orderItemPartiallyCompletedDomainEvent);
        AddDomainEvent(orderItemPartiallyCompletedDomainEvent);
    }

    public void CompleteExam(
        OrderItemId orderItemId,
        DateTime completedAt
    )
    {
        OrderItemCompletedDomainEvent orderItemCompletedDomainEvent = new(
            orderItemId.Value,
            completedAt
        );
        Apply(orderItemCompletedDomainEvent);
        AddDomainEvent(orderItemCompletedDomainEvent);
    }

    public void RejectExam(
        OrderItemId orderItemId,
        string reason,
        DateTime rejectedAt
    )
    {
        OrderItemRejectedDomainEvent orderItemRejectedDomainEvent = new(
            orderItemId.Value,
            reason,
            rejectedAt
        );
        Apply(orderItemRejectedDomainEvent);
        AddDomainEvent(orderItemRejectedDomainEvent);
    }


    private void When(OrderCreatedDomainEvent domainEvent)
    {
        Id = domainEvent.OrderId;
        _patientId = new(domainEvent.PatientId);
        _requestedBy = new(domainEvent.RequestedBy);
        _orderPriority = OrderPriority.Of(domainEvent.OrderPriority);
        _requestedAt = domainEvent.RequestedAt;
    }
    private void When(OrderItemRequestedDomainEvent domainEvent)
        => _items.Add(OrderItem.Request(domainEvent));
    private void When(OrderItemCanceledDomainEvent domainEvent)
        => _items.Single(i => i.OrderItemId.Value == domainEvent.OrderItemId).Cancel(domainEvent);
    private void When(OrderItemPlacedOnHoldDomainEvent domainEvent)
        => _items.Single(i => i.OrderItemId.Value == domainEvent.OrderItemId).PlaceOnHold(domainEvent);
    private void When(OrderItemAcceptedDomainEvent domainEvent)
        => _items.Single(i => i.OrderItemId.Value == domainEvent.OrderItemId).Accept(domainEvent);
    private void When(OrderItemPlacedInProgressDomainEvent domainEvent)
        => _items.Single(i => i.OrderItemId.Value == domainEvent.OrderItemId).PlaceInProgress(domainEvent);
    private void When(OrderItemPartiallyCompletedDomainEvent domainEvent)
        => _items.Single(i => i.OrderItemId.Value == domainEvent.OrderItemId).PartiallyComplete(domainEvent);
    private void When(OrderItemCompletedDomainEvent domainEvent)
        => _items.Single(i => i.OrderItemId.Value == domainEvent.OrderItemId).Complete(domainEvent);
    private void When(OrderItemRejectedDomainEvent domainEvent)
        => _items.Single(i => i.OrderItemId.Value == domainEvent.OrderItemId).Reject(domainEvent);

}
