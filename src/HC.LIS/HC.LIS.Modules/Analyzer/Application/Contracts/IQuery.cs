using MediatR;

namespace HC.LIS.Modules.Analyzer.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
