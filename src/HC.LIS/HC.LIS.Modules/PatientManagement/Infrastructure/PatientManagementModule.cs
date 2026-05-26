using Autofac;
using MediatR;
using HC.LIS.Modules.PatientManagement.Application.Contracts;
using HC.LIS.Modules.PatientManagement.Infrastructure.Configurations;
using HC.LIS.Modules.PatientManagement.Infrastructure.Configurations.Processing;

namespace HC.LIS.Modules.PatientManagement.Infrastructure;

public class PatientManagementModule : IPatientManagementModule
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
        using (ILifetimeScope scope = PatientManagementCompositionRoot.BeginLifetimeScope())
        {
            IMediator mediator = scope.Resolve<IMediator>();
            return await mediator.Send(query).ConfigureAwait(false);
        }
    }
}
