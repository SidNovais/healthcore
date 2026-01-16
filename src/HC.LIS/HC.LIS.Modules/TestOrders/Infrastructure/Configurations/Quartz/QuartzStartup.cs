using System.Collections.Specialized;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing.Inbox;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing.InternalCommands;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing.Outbox;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using Serilog;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Quartz;

internal static class QuartzStartup
{
    private static IScheduler _scheduler;

    internal static void Initialize(ILogger logger, long? internalProcessingPoolingInterval)
    {
        logger.Information("Quartz starting...");

        var schedulerConfiguration = new NameValueCollection
      {
          { "quartz.scheduler.instanceName", "TestOrders" }
      };

        StdSchedulerFactory schedulerFactory = new(schedulerConfiguration);
        _scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();

        LogProvider.SetCurrentLogProvider(new SerilogLogProvider(logger));

        _scheduler.Start().GetAwaiter().GetResult();

        IJobDetail processOutboxJob = JobBuilder.Create<ProcessOutboxJob>().Build();

        ITrigger trigger;
        if (internalProcessingPoolingInterval.HasValue)
        {
            trigger =
                TriggerBuilder
                    .Create()
                    .StartNow()
                    .WithSimpleSchedule(x =>
                        x.WithInterval(TimeSpan.FromMilliseconds(internalProcessingPoolingInterval.Value))
                            .RepeatForever())
                    .Build();
        }
        else
        {
            trigger =
                TriggerBuilder
                    .Create()
                    .StartNow()
                    .WithCronSchedule("0/2 * * ? * *")
                    .Build();
        }

        _scheduler
            .ScheduleJob(processOutboxJob, trigger)
            .GetAwaiter().GetResult();

        IJobDetail processInboxJob = JobBuilder.Create<ProcessInboxJob>().Build();
        ITrigger processInboxTrigger =
            TriggerBuilder
                .Create()
                .StartNow()
                .WithCronSchedule("0/2 * * ? * *")
                .Build();

        _scheduler
            .ScheduleJob(processInboxJob, processInboxTrigger)
            .GetAwaiter().GetResult();

        IJobDetail processInternalCommandsJob = JobBuilder.Create<ProcessInternalCommandsJob>().Build();
        ITrigger triggerCommandsProcessing =
            TriggerBuilder
                .Create()
                .StartNow()
                .WithCronSchedule("0/2 * * ? * *")
                .Build();
        _scheduler.ScheduleJob(processInternalCommandsJob, triggerCommandsProcessing).GetAwaiter().GetResult();
        logger.Information("Quartz started.");
    }

    internal static void StopQuartz()
    {
        _scheduler.Shutdown();
    }
}
