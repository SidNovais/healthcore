using Autofac;
using HC.Core.Application;
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.PatientManagement.Application.Patients.RegisterPatient;
using HC.LIS.Modules.PatientManagement.Application.Patients.UpdatePatient;
using HC.LIS.Modules.PatientManagement.Infrastructure.Configurations.Authentication;
using HC.LIS.Modules.PatientManagement.Infrastructure.Configurations.DataAccess;
using HC.LIS.Modules.PatientManagement.Infrastructure.Configurations.EventBus;
using HC.LIS.Modules.PatientManagement.Infrastructure.Configurations.Logging;
using HC.LIS.Modules.PatientManagement.Infrastructure.Configurations.Mediation;
using HC.LIS.Modules.PatientManagement.Infrastructure.Configurations.Processing;
using HC.LIS.Modules.PatientManagement.Infrastructure.Configurations.Processing.InternalCommands;
using HC.LIS.Modules.PatientManagement.Infrastructure.Configurations.Processing.Outbox;
using HC.LIS.Modules.PatientManagement.Infrastructure.Configurations.Quartz;
using Serilog;

namespace HC.LIS.Modules.PatientManagement.Infrastructure.Configurations;

public class PatientManagementStartup
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
        ILogger moduleLogger = logger.ForContext("Module", "PatientManagement");
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
        domainNotificationsMap.Add("PatientRegisteredNotification", typeof(PatientRegisteredNotification));
        domainNotificationsMap.Add("PatientUpdatedNotification", typeof(PatientUpdatedNotification));
        containerBuilder.RegisterModule(new OutboxModule(domainNotificationsMap));
        BiMap internalCommandsMap = new();
        containerBuilder.RegisterModule(new InternalCommandsModule(internalCommandsMap));
        containerBuilder.RegisterModule(new QuartzModule());
        containerBuilder.RegisterInstance(executionContextAccessor);
        _container = containerBuilder.Build();
        PatientManagementCompositionRoot.SetContainer(_container);
    }
    public static void Stop()
    {
        QuartzStartup.StopQuartz();
    }
}
