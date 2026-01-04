
using MediatR;

namespace HC.LIS.Modules.TestOrders.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
