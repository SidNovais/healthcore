using Autofac;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Queries;
using HC.LIS.Modules.SampleCollection.Application.Contracts;
using MediatR;

namespace HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.Processing;

public class QueryExecutor : IQueryExecutor
{
    public async Task<TResult> GetAsync<TResult>(IQuery<TResult> query)
    {
        using (ILifetimeScope scope = SampleCollectionCompositionRoot.BeginLifetimeScope())
        {
            IMediator mediator = scope.Resolve<IMediator>();
            return await mediator.Send(query).ConfigureAwait(false);
        }
    }
}
