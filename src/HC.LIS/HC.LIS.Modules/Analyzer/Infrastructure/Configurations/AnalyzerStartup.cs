using Autofac;
using HC.Core.Application;
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.AssignWorklistItem;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.CreateAnalyzerSample;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.DispatchSampleInfo;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.ReceiveExamResult;
using HC.LIS.Modules.Analyzer.Infrastructure.Configurations.Authentication;
using HC.LIS.Modules.Analyzer.Infrastructure.Configurations.DataAccess;
using HC.LIS.Modules.Analyzer.Infrastructure.Configurations.EventBus;
using HC.LIS.Modules.Analyzer.Infrastructure.Configurations.Logging;
using HC.LIS.Modules.Analyzer.Infrastructure.Configurations.Mediation;
using HC.LIS.Modules.Analyzer.Infrastructure.Configurations.Processing;
using HC.LIS.Modules.Analyzer.Infrastructure.Configurations.Processing.InternalCommands;
using HC.LIS.Modules.Analyzer.Infrastructure.Configurations.Processing.Outbox;
using HC.LIS.Modules.Analyzer.Infrastructure.Configurations.HL7;
using HC.LIS.Modules.Analyzer.Infrastructure.Configurations.Quartz;
using Serilog;

namespace HC.LIS.Modules.Analyzer.Infrastructure.Configurations;

public class AnalyzerStartup
{
    private static IContainer _container = null!;
    public static void Initialize(
      string databaseConnectionString,
      IExecutionContextAccessor executionContextAccessor,
      ILogger logger,
      IEventsBus? eventBus,
      long? internalProcessingPoolingInterval = null,
      bool enableHl7Checksum = false
    )
    {
        ILogger moduleLogger = logger.ForContext("Module", "Analyzer");
        ConfigureContainer(
          databaseConnectionString,
          executionContextAccessor,
          moduleLogger,
          eventBus,
          enableHl7Checksum
        );
        QuartzStartup.Initialize(moduleLogger, internalProcessingPoolingInterval);
        EventsBusStartup.Initialize(moduleLogger);
    }

    private static void ConfigureContainer(
      string databaseConnectionString,
      IExecutionContextAccessor executionContextAccessor,
      ILogger logger,
      IEventsBus? eventsBus,
      bool enableHl7Checksum
    )
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterModule(new LoggingModule(logger));
        containerBuilder.RegisterModule(new DataAccessModule(databaseConnectionString));
        containerBuilder.RegisterModule(new ApplicationModule());
        containerBuilder.RegisterModule(new ProcessingModule());
        containerBuilder.RegisterModule(new EventsBusModule(eventsBus));
        containerBuilder.RegisterModule(new MediatorModule());
        containerBuilder.RegisterModule(new AuthenticationModule());
        var domainNotificationsMap = new BiMap();
        domainNotificationsMap.Add("AnalyzerSampleCreatedNotification",  typeof(AnalyzerSampleCreatedNotification));
        domainNotificationsMap.Add("WorklistItemAssignedNotification",    typeof(WorklistItemAssignedNotification));
        domainNotificationsMap.Add("SampleInfoDispatchedNotification",    typeof(SampleInfoDispatchedNotification));
        domainNotificationsMap.Add("ExamResultReceivedNotification",      typeof(ExamResultReceivedNotification));
        containerBuilder.RegisterModule(new OutboxModule(domainNotificationsMap));
        BiMap internalCommandsMap = new();
        internalCommandsMap.Add("CreateAnalyzerSampleBySampleCollectedCommand", typeof(CreateAnalyzerSampleBySampleCollectedCommand));
        internalCommandsMap.Add("AssignWorklistItemByBarcodeAndExamCodeCommand", typeof(AssignWorklistItemByBarcodeAndExamCodeCommand));
        containerBuilder.RegisterModule(new InternalCommandsModule(internalCommandsMap));
        containerBuilder.RegisterModule(new QuartzModule());
        containerBuilder.RegisterModule(new HL7Module(enableHl7Checksum));
        containerBuilder.RegisterInstance(executionContextAccessor);
        _container = containerBuilder.Build();
        AnalyzerCompositionRoot.SetContainer(_container);
    }
    public static void Stop()
    {
        QuartzStartup.StopQuartz();
    }
}
