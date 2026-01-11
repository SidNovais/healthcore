using Quartz;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing.Inbox;

[DisallowConcurrentExecution]
public class ProcessInboxJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await CommandsExecutor.Execute(new ProcessInboxCommand()).ConfigureAwait(false);
    }
}
