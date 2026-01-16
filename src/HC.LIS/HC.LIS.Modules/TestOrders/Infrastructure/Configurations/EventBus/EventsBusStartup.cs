using Autofac;
using Serilog;
using HC.Core.Infrastructure.EventBus;
using HC.COre.Infrastructure.EventBus;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.EventBus;

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
        IEventsBus eventBus = TestOrdersCompositionRoot.BeginLifetimeScope().Resolve<IEventsBus>();
    }

    private static void SubscribeToIntegrationEvent<T>(IEventsBus eventBus, ILogger logger)
        where T : IntegrationEvent
    {
        logger.Information("Subscribe to {@IntegrationEvent}", typeof(T).FullName);
        eventBus.Subscribe(
            new IntegrationEventGenericHandler<T>());
    }
}
