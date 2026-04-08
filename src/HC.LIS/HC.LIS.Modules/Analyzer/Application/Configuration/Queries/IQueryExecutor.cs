using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Modules.Analyzer.Application.Configuration.Queries;

public interface IQueryExecutor
{
    Task<TResult> GetAsync<TResult>(IQuery<TResult> query);
}
