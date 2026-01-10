using Autofac;
using HC.LIS.Modules.TestOrders.Application.Configuration.Queries;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using MediatR;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing;

public class Queries : IQueryExecutor
{
    public async Task<TResult> GetAsync<TResult>(IQuery<TResult> query)
    {
        using (ILifetimeScope scope = TestOrdersCompositionRoot.BeginLifetimeScope())
        {
            IMediator mediator = scope.Resolve<IMediator>();
            return await mediator.Send(query).ConfigureAwait(false);
        }
    }
}
