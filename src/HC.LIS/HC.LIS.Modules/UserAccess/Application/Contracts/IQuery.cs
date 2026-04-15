using MediatR;

namespace HC.LIS.Modules.UserAccess.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
