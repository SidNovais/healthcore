using System;
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

    private void When(OrderCreatedDomainEvent domainEvent)
    {
        Id = domainEvent.Id;
        _patientId = new(domainEvent.PatientId);
        _requestedBy = new(domainEvent.RequestedBy);
        _orderPriority = OrderPriority.Of(domainEvent.OrderPriority);
        _requestedAt = domainEvent.RequestedAt;
    }

}
