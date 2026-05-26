using Autofac;
using HC.LIS.Modules.PatientManagement.Application.Configuration.Queries;
using HC.LIS.Modules.PatientManagement.Application.Contracts;
using MediatR;

namespace HC.LIS.Modules.PatientManagement.Infrastructure.Configurations.Processing;

public class QueryExecutor : IQueryExecutor
{
    public async Task<TResult> GetAsync<TResult>(IQuery<TResult> query)
    {
        using (ILifetimeScope scope = PatientManagementCompositionRoot.BeginLifetimeScope())
        {
            IMediator mediator = scope.Resolve<IMediator>();
            return await mediator.Send(query).ConfigureAwait(false);
        }
    }
}
