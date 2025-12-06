using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace HC.Core.Infrastructure.DomainEventsDispatching;

public class UnitOfWorkCommandHandlerDecorator<T>(
    IRequestHandler<T> decorated,
    IUnitOfWork unitOfWork
) : IRequestHandler<T> where T : IRequest
{
    private readonly IRequestHandler<T> _decorated = decorated;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task Handle(
        T command,
        CancellationToken cancellationToken
    )
    {
        await _decorated.Handle(command, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.CommitAsync(null, cancellationToken).ConfigureAwait(false);
    }
}
