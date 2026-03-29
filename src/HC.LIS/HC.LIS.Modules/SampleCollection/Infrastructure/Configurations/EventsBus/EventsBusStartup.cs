using Autofac;
using Serilog;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.EventBus;

internal static class EventsBusStartup
{
    internal static void Initialize(
        ILogger logger
    )
    {
        SubscribeToIntegrationEvents(logger);
    }

    private static void SubscribeToIntegrationEvents(ILogger logger)
    {
        IEventsBus eventBus = SampleCollectionCompositionRoot.BeginLifetimeScope().Resolve<IEventsBus>();
        SubscribeToIntegrationEvent<OrderItemAcceptedIntegrationEvent>(eventBus, logger);
    }

    private static void SubscribeToIntegrationEvent<T>(IEventsBus eventBus, ILogger logger)
        where T : IntegrationEvent
    {
        logger.Information("Subscribe to {@IntegrationEvent}", typeof(T).FullName);
        eventBus.Subscribe(
            new IntegrationEventGenericHandler<T>());
    }
}
