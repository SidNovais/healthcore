using HC.LIS.Modules.LabAnalysis.Application.Contracts;

namespace HC.LIS.Modules.LabAnalysis.Application.Configuration.Queries;

public interface IQueryExecutor
{
    Task<TResult> GetAsync<TResult>(IQuery<TResult> query);
}
