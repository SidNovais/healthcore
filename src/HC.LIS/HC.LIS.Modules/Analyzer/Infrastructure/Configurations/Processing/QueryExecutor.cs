using Autofac;
using HC.LIS.Modules.Analyzer.Application.Configuration.Queries;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using MediatR;

namespace HC.LIS.Modules.Analyzer.Infrastructure.Configurations.Processing;

public class QueryExecutor : IQueryExecutor
{
    public async Task<TResult> GetAsync<TResult>(IQuery<TResult> query)
    {
        using (ILifetimeScope scope = AnalyzerCompositionRoot.BeginLifetimeScope())
        {
            IMediator mediator = scope.Resolve<IMediator>();
            return await mediator.Send(query).ConfigureAwait(false);
        }
    }
}
