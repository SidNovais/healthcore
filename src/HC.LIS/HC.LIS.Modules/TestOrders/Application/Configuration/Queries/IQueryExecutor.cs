using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Configuration.Queries;

public interface IQueries
{
    Task<TResult> GetAsync<TResult>(IQuery<TResult> query);
}
