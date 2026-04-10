using Autofac;
using Serilog;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.LabAnalysis.IntegrationEvents;
using HC.LIS.Modules.SampleCollection.IntegrationEvents;

namespace HC.LIS.Modules.Analyzer.Infrastructure.Configurations.EventBus;

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
        IEventsBus eventBus = AnalyzerCompositionRoot.BeginLifetimeScope().Resolve<IEventsBus>();
        SubscribeToIntegrationEvent<SampleCollectedIntegrationEvent>(eventBus, logger);
        SubscribeToIntegrationEvent<WorklistItemCreatedIntegrationEvent>(eventBus, logger);
    }

    private static void SubscribeToIntegrationEvent<T>(IEventsBus eventBus, ILogger logger)
        where T : IntegrationEvent
    {
        logger.Information("Subscribe to {@IntegrationEvent}", typeof(T).FullName);
        eventBus.Subscribe(
            new IntegrationEventGenericHandler<T>());
    }
}
