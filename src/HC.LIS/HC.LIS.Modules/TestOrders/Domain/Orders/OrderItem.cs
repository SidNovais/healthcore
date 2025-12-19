using System;
using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Domain.Orders.Events;

namespace HC.LIS.Modules.TestOrders.Domain.Orders;

public class OrderItem : Entity
{
    internal OrderItemId OrderItemId { get; private set; }
    internal SpecimenRequirement _speciamentRequirement;
    internal OrderItemStatus _status;
    internal DateTime _requestedAt;

    public static OrderItem Request(
        OrderItemRequestedDomainEvent domainEvent
    )
    {
        OrderItem order = new();
        order.Apply(domainEvent);
        return order;
    }

    private void Apply(IDomainEvent domainEvent) => When((dynamic)domainEvent);

    private void When(OrderItemRequestedDomainEvent domainEvent)
    {
        OrderItemId = new(domainEvent.OrderItemId);
        _speciamentRequirement = SpecimenRequirement.Of(
            domainEvent.SpecimenMnemonic,
            domainEvent.MaterialType,
            domainEvent.ContainerType
        );
        _status = OrderItemStatus.Requested;
        _requestedAt = domainEvent.RequestedAt;
    }
}
