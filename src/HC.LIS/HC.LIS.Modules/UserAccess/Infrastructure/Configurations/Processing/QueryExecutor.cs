using Autofac;
using HC.LIS.Modules.UserAccess.Application.Configuration.Queries;
using HC.LIS.Modules.UserAccess.Application.Contracts;
using MediatR;

namespace HC.LIS.Modules.UserAccess.Infrastructure.Configurations.Processing;

public class QueryExecutor : IQueryExecutor
{
    public async Task<TResult> GetAsync<TResult>(IQuery<TResult> query)
    {
        using (ILifetimeScope scope = UserAccessCompositionRoot.BeginLifetimeScope())
        {
            IMediator mediator = scope.Resolve<IMediator>();
            return await mediator.Send(query).ConfigureAwait(false);
        }
    }
}
