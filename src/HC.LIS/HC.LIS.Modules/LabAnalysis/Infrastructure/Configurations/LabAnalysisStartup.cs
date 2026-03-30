using Autofac;
using HC.Core.Application;
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CompleteWorklistItem;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CreateWorklistItem;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GenerateReport;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.RecordAnalysisResult;
using HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.Authentication;
using HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.DataAccess;
using HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.EventBus;
using HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.Logging;
using HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.Mediation;
using HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.Processing;
using HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.Processing.InternalCommands;
using HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.Processing.Outbox;
using HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.Quartz;
using Serilog;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations;

public class LabAnalysisStartup
{
    private static IContainer _container = null!;
    public static void Initialize(
      string databaseConnectionString,
      IExecutionContextAccessor executionContextAccessor,
      ILogger logger,
      IEventsBus? eventBus,
      long? internalProcessingPoolingInterval = null
    )
    {
        ILogger moduleLogger = logger.ForContext("Module", "LabAnalysis");
        ConfigureContainer(
          databaseConnectionString,
          executionContextAccessor,
          moduleLogger,
          eventBus
        );
        QuartzStartup.Initialize(moduleLogger, internalProcessingPoolingInterval);
        EventsBusStartup.Initialize(moduleLogger);
    }

    private static void ConfigureContainer(
      string databaseConnectionString,
      IExecutionContextAccessor executionContextAccessor,
      ILogger logger,
      IEventsBus? eventsBus
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
        domainNotificationsMap.Add("WorklistItemCreatedNotification",      typeof(WorklistItemCreatedNotification));
        domainNotificationsMap.Add("AnalysisResultRecordedNotification",   typeof(AnalysisResultRecordedNotification));
        domainNotificationsMap.Add("ReportGeneratedNotification",          typeof(ReportGeneratedNotification));
        domainNotificationsMap.Add("WorklistItemCompletedNotification",    typeof(WorklistItemCompletedNotification));
        containerBuilder.RegisterModule(new OutboxModule(domainNotificationsMap));
        BiMap internalCommandsMap = new();
        internalCommandsMap.Add("CreateWorklistItemCommand",    typeof(CreateWorklistItemCommand));
        internalCommandsMap.Add("RecordAnalysisResultCommand", typeof(RecordAnalysisResultCommand));
        internalCommandsMap.Add("GenerateReportCommand",       typeof(GenerateReportCommand));
        containerBuilder.RegisterModule(new InternalCommandsModule(internalCommandsMap));
        containerBuilder.RegisterModule(new QuartzModule());
        containerBuilder.RegisterInstance(executionContextAccessor);
        _container = containerBuilder.Build();
        LabAnalysisCompositionRoot.SetContainer(_container);
    }
    public static void Stop()
    {
        QuartzStartup.StopQuartz();
    }
}
