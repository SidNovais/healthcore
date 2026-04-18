using HC.Core.Domain;
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.InternalCommands;
using HC.LIS.Modules.UserAccess.Application.Configuration.Commands;
using HC.LIS.Modules.UserAccess.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace HC.LIS.Modules.UserAccess.Infrastructure.Configurations.Processing;

internal class UnitOfWorkCommandHandlerWithResultDecorator<T, TResult>(
    ICommandHandler<T, TResult> decorated,
    IUnitOfWork unitOfWork,
    UserAccessContext context
) : ICommandHandler<T, TResult>
    where T : ICommand<TResult>
{
    private readonly ICommandHandler<T, TResult> _decorated = decorated;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly UserAccessContext _context = context;

    public async Task<TResult> Handle(T command, CancellationToken cancellationToken)
    {
        TResult result = await _decorated.Handle(command, cancellationToken).ConfigureAwait(false);

        if (command is InternalCommandBase<TResult>)
        {
            InternalCommand? internalCommand = await _context.InternalCommands
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
                .ConfigureAwait(false);
            if (internalCommand is not null)
                internalCommand.ProcessedDate = SystemClock.Now;
        }

        await _unitOfWork.CommitAsync(null, cancellationToken).ConfigureAwait(false);

        return result;
    }
}
