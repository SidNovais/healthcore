using MediatR;

namespace HC.LIS.Modules.SampleCollection.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
