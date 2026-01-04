using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Configuration.Commands;

public interface ICommandsScheduler
{
    Task EnqueueAsync(ICommand command);
    Task EnqueueAsync<T>(ICommand<T> command);
}
