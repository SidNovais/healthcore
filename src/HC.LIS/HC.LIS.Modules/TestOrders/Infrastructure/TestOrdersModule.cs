using Autofac;
using MediatR;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing;

namespace HC.LIS.Modules.TestOrders.Infrastructure;

public class TestOrdersModule : ITestOrdersModule
{
    public async Task<TResult> ExecuteCommandAsync<TResult>(ICommand<TResult> command)
    {
        return await CommandsExecutor.Execute(command).ConfigureAwait(false);
    }

    public async Task ExecuteCommandAsync(ICommand command)
    {
        await CommandsExecutor.Execute(command).ConfigureAwait(false);
    }

    public async Task<TResult> ExecuteQueryAsync<TResult>(IQuery<TResult> query)
    {
        using (ILifetimeScope scope = TestOrdersCompositionRoot.BeginLifetimeScope())
        {
            IMediator mediator = scope.Resolve<IMediator>();
            return await mediator.Send(query).ConfigureAwait(false);
        }
    }
}
