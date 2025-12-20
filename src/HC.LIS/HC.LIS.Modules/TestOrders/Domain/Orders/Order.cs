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
        SpecimenRequirement specimenRequirement,
        DateTime requestedAt
    )
    {
        OrderItemRequestedDomainEvent orderItemRequestedDomainEvent = new(
            Guid.CreateVersion7(),
            specimenRequirement.SpecimenMnemonic,
            specimenRequirement.MaterialType,
            specimenRequirement.ContainerType,
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


    private void When(OrderCreatedDomainEvent domainEvent)
    {
        Id = domainEvent.Id;
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

}
