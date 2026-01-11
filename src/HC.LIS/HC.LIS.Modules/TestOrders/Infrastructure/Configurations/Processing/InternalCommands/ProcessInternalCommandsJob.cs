using Quartz;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing.InternalCommands;

[DisallowConcurrentExecution]
public class ProcessInternalCommandsJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await CommandsExecutor.Execute(new ProcessInternalCommandsCommand()).ConfigureAwait(false);
    }
}
