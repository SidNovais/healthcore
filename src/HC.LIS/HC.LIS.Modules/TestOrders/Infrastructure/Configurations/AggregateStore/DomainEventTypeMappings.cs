using HC.LIS.Modules.TestOrders.Domain.Orders.Events;
using HC.LIS.Modules.TestOrders.Domain.Physicians.Events;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.AggregateStore;

internal static class DomainEventTypeMappings
{
    internal static IDictionary<string, Type> Dictionary { get; }

    static DomainEventTypeMappings()
    {
        Dictionary = new Dictionary<string, Type>
        {
            { "OrderCreatedDomainEvent", typeof(OrderCreatedDomainEvent) },
            { "OrderItemAcceptedDomainEvent", typeof(OrderItemAcceptedDomainEvent) },
            { "OrderItemCanceledDomainEvent", typeof(OrderItemCanceledDomainEvent) },
            { "OrderItemCompletedDomainEvent", typeof(OrderItemCompletedDomainEvent) },
            { "OrderItemPartiallyCompletedDomainEvent", typeof(OrderItemPartiallyCompletedDomainEvent) },
            { "OrderItemPlacedInProgressDomainEvent", typeof(OrderItemPlacedInProgressDomainEvent) },
            { "OrderItemPlacedOnHoldDomainEvent", typeof(OrderItemPlacedOnHoldDomainEvent) },
            { "OrderItemRejectedDomainEvent", typeof(OrderItemRejectedDomainEvent) },
            { "OrderItemRequestedDomainEvent", typeof(OrderItemRequestedDomainEvent) },
            { "PhysicianRegisteredDomainEvent", typeof(PhysicianRegisteredDomainEvent) },
            { "PhysicianUpdatedDomainEvent", typeof(PhysicianUpdatedDomainEvent) },
            { "PhysicianDeactivatedDomainEvent", typeof(PhysicianDeactivatedDomainEvent) },
            { "PhysicianReactivatedDomainEvent", typeof(PhysicianReactivatedDomainEvent) },
        };
    }
}
