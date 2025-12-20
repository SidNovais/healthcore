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
    internal DateTime _canceledAt;
    internal DateTime _onHoldAt;
    internal DateTime _acceptedAt;

    public static OrderItem Request(
        OrderItemRequestedDomainEvent domainEvent
    )
    {
        OrderItem order = new();
        order.Apply(domainEvent);
        return order;
    }

    public void Cancel(OrderItemCanceledDomainEvent domainEvent)
        => Apply(domainEvent);
    public void PlaceOnHold(OrderItemPlacedOnHoldDomainEvent domainEvent)
        => Apply(domainEvent);
    public void Accept(OrderItemAcceptedDomainEvent domainEvent)
        => Apply(domainEvent);

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
}
