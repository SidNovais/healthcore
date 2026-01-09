using HC.Core.Infrastructure;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing;

internal class UnitOfWorkCommandHandlerDecorator<T>(
    ICommandHandler<T> decorated,
    IUnitOfWork unitOfWork
) : ICommandHandler<T>
    where T : ICommand
{
    private readonly ICommandHandler<T> _decorated = decorated;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(T command, CancellationToken cancellationToken)
    {
        await _decorated.Handle(command, cancellationToken).ConfigureAwait(false);

        Guid? internalCommandId = null;
        if (command is InternalCommandBase internalCommandBase)
        {
            internalCommandId = internalCommandBase.Id;
        }

        await _unitOfWork.CommitAsync(
          internalCommandId,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
    }
}
