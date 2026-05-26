using MediatR;

namespace HC.LIS.Modules.PatientManagement.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
