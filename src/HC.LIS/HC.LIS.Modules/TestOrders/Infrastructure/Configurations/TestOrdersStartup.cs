using Autofac;
using HC.Core.Application;
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.TestOrders.Application.Orders.AcceptExam;
using HC.LIS.Modules.TestOrders.Application.Orders.CancelExam;
using HC.LIS.Modules.TestOrders.Application.Orders.CreateOrder;
using HC.LIS.Modules.TestOrders.Application.Orders.RejectExam;
using HC.LIS.Modules.TestOrders.Application.Orders.RequestExam;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Authentication;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.DataAccess;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.EventBus;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Logging;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Mediation;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing.Outbox;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Quartz;
using Serilog;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations;

public class TestOrdersStartup
{
    private static IContainer _container;
    public static void Initialize(
      string databaseConnectionString,
      IExecutionContextAccessor executionContextAccessor,
      ILogger logger,
      IEventsBus? eventBus,
      long? internalProcessingPoolingInterval = null
    )
    {
        ILogger moduleLogger = logger.ForContext("Module", "TestOrders");
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
        domainNotificationsMap.Add("OrderCreatedNotification", typeof(OrderCreatedNotification));
        domainNotificationsMap.Add("ExamRequestedNotification", typeof(ExamRequestedNotification));
        domainNotificationsMap.Add("ExamCanceledNotification", typeof(ExamCanceledNotification));
        domainNotificationsMap.Add("ExamAcceptedNotification", typeof(ExamAcceptedNotification));
        domainNotificationsMap.Add("ExamRejectedNotification", typeof(ExamRejectedNotification));
        containerBuilder.RegisterModule(new OutboxModule(domainNotificationsMap));
        BiMap internalCommandsMap = new();
        containerBuilder.RegisterModule(new InternalCommandsModule(internalCommandsMap));
        containerBuilder.RegisterModule(new QuartzModule());
        containerBuilder.RegisterInstance(executionContextAccessor);
        _container = containerBuilder.Build();
        TestOrdersCompositionRoot.SetContainer(_container);
    }
    public static void Stop()
    {
        QuartzStartup.StopQuartz();
    }
}
