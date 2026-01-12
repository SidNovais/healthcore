using Quartz;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing.Outbox;

[DisallowConcurrentExecution]
public class ProcessOutboxJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await CommandsExecutor.Execute(new ProcessOutboxCommand()).ConfigureAwait(false);
    }
}
