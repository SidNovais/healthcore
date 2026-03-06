using Autofac;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Queries;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using MediatR;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.Processing;

public class QueryExecutor : IQueryExecutor
{
    public async Task<TResult> GetAsync<TResult>(IQuery<TResult> query)
    {
        using (ILifetimeScope scope = LabAnalysisCompositionRoot.BeginLifetimeScope())
        {
            IMediator mediator = scope.Resolve<IMediator>();
            return await mediator.Send(query).ConfigureAwait(false);
        }
    }
}
