using Autofac;
using MediatR;
using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing;

internal static class CommandsExecutor
{
    internal static async Task Execute(ICommand command)
    {
        using (ILifetimeScope scope = TestOrdersCompositionRoot.BeginLifetimeScope())
        {
            IMediator mediator = scope.Resolve<IMediator>();
            await mediator.Send(command).ConfigureAwait(false);
        }
    }

    internal static async Task<TResult> Execute<TResult>(ICommand<TResult> command)
    {
        using (ILifetimeScope scope = TestOrdersCompositionRoot.BeginLifetimeScope())
        {
            IMediator mediator = scope.Resolve<IMediator>();
            return await mediator.Send(command).ConfigureAwait(false);
        }
    }
}
