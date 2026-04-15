using Autofac;
using MediatR;
using HC.LIS.Modules.UserAccess.Application.Contracts;
using HC.LIS.Modules.UserAccess.Infrastructure.Configurations;
using HC.LIS.Modules.UserAccess.Infrastructure.Configurations.Processing;

namespace HC.LIS.Modules.UserAccess.Infrastructure;

public class UserAccessModule : IUserAccessModule
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
        using (ILifetimeScope scope = UserAccessCompositionRoot.BeginLifetimeScope())
        {
            IMediator mediator = scope.Resolve<IMediator>();
            return await mediator.Send(query).ConfigureAwait(false);
        }
    }
}
