using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.Application.Configuration.Queries;

public interface IQueryExecutor
{
    Task<TResult> GetAsync<TResult>(IQuery<TResult> query);
}
