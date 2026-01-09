using HC.Core.Infrastructure;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing;

internal class UnitOfWorkCommandHandlerWithResultDecorator<T, TResult>(
    ICommandHandler<T, TResult> decorated,
    IUnitOfWork unitOfWork
) : ICommandHandler<T, TResult>
    where T : ICommand<TResult>
{
    private readonly ICommandHandler<T, TResult> _decorated = decorated;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<TResult> Handle(T command, CancellationToken cancellationToken)
    {
        TResult? result = await _decorated.Handle(command, cancellationToken).ConfigureAwait(false);

        Guid? internalCommandId = null;
        if (command is InternalCommandBase<TResult> internalCommandBase)
        {
            internalCommandId = internalCommandBase.Id;
        }

        await _unitOfWork.CommitAsync(
          internalCommandId,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        return result;
    }
}
