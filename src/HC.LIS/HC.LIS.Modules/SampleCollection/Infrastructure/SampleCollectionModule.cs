using Autofac;
using MediatR;
using HC.LIS.Modules.SampleCollection.Application.Contracts;
using HC.LIS.Modules.SampleCollection.Infrastructure.Configurations;
using HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.Processing;

namespace HC.LIS.Modules.SampleCollection.Infrastructure;

public class SampleCollectionModule : ISampleCollectionModule
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
        using (ILifetimeScope scope = SampleCollectionCompositionRoot.BeginLifetimeScope())
        {
            IMediator mediator = scope.Resolve<IMediator>();
            return await mediator.Send(query).ConfigureAwait(false);
        }
    }
}
