using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Contracts;

public interface ITestOrdersModule
  {
    Task<TResult> ExecuteCommandAsync<TResult>(ICommand<TResult> command);
    Task ExecuteCommandAsync(ICommand command);
    Task<TResult> ExecuteQueryAsync<TResult>(IQuery<TResult> query);
  }
