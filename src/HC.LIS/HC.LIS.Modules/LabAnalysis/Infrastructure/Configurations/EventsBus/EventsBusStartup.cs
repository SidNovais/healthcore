using Autofac;
using Serilog;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.Analyzer.IntegrationEvents;
using HC.LIS.Modules.PatientManagement.IntegrationEvents;
using HC.LIS.Modules.SampleCollection.IntegrationEvents;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.EventBus;

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
        IEventsBus eventBus = LabAnalysisCompositionRoot.BeginLifetimeScope().Resolve<IEventsBus>();
        SubscribeToIntegrationEvent<SampleCollectedIntegrationEvent>(eventBus, logger);
        SubscribeToIntegrationEvent<ExamResultReceivedIntegrationEvent>(eventBus, logger);
        SubscribeToIntegrationEvent<PatientRegisteredIntegrationEvent>(eventBus, logger);
        SubscribeToIntegrationEvent<PatientUpdatedIntegrationEvent>(eventBus, logger);
        SubscribeToIntegrationEvent<PatientAnonymizedIntegrationEvent>(eventBus, logger);
    }

    private static void SubscribeToIntegrationEvent<T>(IEventsBus eventBus, ILogger logger)
        where T : IntegrationEvent
    {
        logger.Information("Subscribe to {@IntegrationEvent}", typeof(T).FullName);
        eventBus.Subscribe(
            new IntegrationEventGenericHandler<T>());
    }
}
