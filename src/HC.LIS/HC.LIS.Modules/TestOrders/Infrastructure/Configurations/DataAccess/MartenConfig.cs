using HC.LIS.Modules.TestOrders.Domain.Orders.Events;
using JasperFx;
using JasperFx.Events;
using Marten;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.DataAccess;

public static class MartenConfig
{
    public static IDocumentStore BuildDocumentStore(string connectionString)
    {
        var store = DocumentStore.For(options =>
        {
            options.Connection(connectionString);
            options.DatabaseSchemaName = "test_orders";
            options.AutoCreateSchemaObjects = AutoCreate.None;
            options.Events.StreamIdentity = StreamIdentity.AsString;
            // Register all your event types
            options.Events.AddEventType<OrderCreatedDomainEvent>();
            options.Events.AddEventType<OrderItemAcceptedDomainEvent>();
            options.Events.AddEventType<OrderItemCanceledDomainEvent>();
            options.Events.AddEventType<OrderItemCompletedDomainEvent>();
            options.Events.AddEventType<OrderItemPartiallyCompletedDomainEvent>();
            options.Events.AddEventType<OrderItemPlacedInProgressDomainEvent>();
            options.Events.AddEventType<OrderItemPlacedOnHoldDomainEvent>();
            options.Events.AddEventType<OrderItemRejectedDomainEvent>();
            options.Events.AddEventType<OrderItemRequestedDomainEvent>();
            // Add projections if you have them
            // options.Projections.Snapshot<UserAggregate>();
        });
        return store;
    }
}
