using Autofac;
using HC.Core.Application;
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.SampleCollection.Application.Collections.CreateCollectionRequest;
using HC.LIS.Modules.SampleCollection.Application.Collections.CallPatient;
using HC.LIS.Modules.SampleCollection.Application.Collections.CreateBarcode;
using HC.LIS.Modules.SampleCollection.Application.Collections.MovePatientToWaiting;
using HC.LIS.Modules.SampleCollection.Application.Collections.RecordSampleCollection;
using HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.Authentication;
using HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.DataAccess;
using HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.EventBus;
using HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.Logging;
using HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.Mediation;
using HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.Processing;
using HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.Processing.InternalCommands;
using HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.Processing.Outbox;
using HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.Quartz;
using Serilog;

namespace HC.LIS.Modules.SampleCollection.Infrastructure.Configurations;

public class SampleCollectionStartup
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
        ILogger moduleLogger = logger.ForContext("Module", "SampleCollection");
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
        domainNotificationsMap.Add("PatientArrivedNotification", typeof(PatientArrivedNotification));
        domainNotificationsMap.Add("PatientWaitingNotification", typeof(PatientWaitingNotification));
        domainNotificationsMap.Add("PatientCalledNotification", typeof(PatientCalledNotification));
        domainNotificationsMap.Add("BarcodeCreatedNotification", typeof(BarcodeCreatedNotification));
        domainNotificationsMap.Add("SampleCollectedNotification", typeof(SampleCollectedNotification));
        containerBuilder.RegisterModule(new OutboxModule(domainNotificationsMap));
        BiMap internalCommandsMap = new();
        containerBuilder.RegisterModule(new InternalCommandsModule(internalCommandsMap));
        containerBuilder.RegisterModule(new QuartzModule());
        containerBuilder.RegisterInstance(executionContextAccessor);
        _container = containerBuilder.Build();
        SampleCollectionCompositionRoot.SetContainer(_container);
    }
    public static void Stop()
    {
        QuartzStartup.StopQuartz();
    }
}
