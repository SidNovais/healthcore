using HC.Core.Domain;
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.InternalCommands;
using HC.LIS.Modules.UserAccess.Application.Configuration.Commands;
using HC.LIS.Modules.UserAccess.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace HC.LIS.Modules.UserAccess.Infrastructure.Configurations.Processing;

internal class UnitOfWorkCommandHandlerDecorator<T>(
    ICommandHandler<T> decorated,
    IUnitOfWork unitOfWork,
    UserAccessContext context
) : ICommandHandler<T>
    where T : ICommand
{
    private readonly ICommandHandler<T> _decorated = decorated;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly UserAccessContext _context = context;

    public async Task Handle(T command, CancellationToken cancellationToken)
    {
        await _decorated.Handle(command, cancellationToken).ConfigureAwait(false);

        if (command is InternalCommandBase)
        {
            InternalCommand? internalCommand = await _context.InternalCommands
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
                .ConfigureAwait(false);
            if (internalCommand is not null)
                internalCommand.ProcessedDate = SystemClock.Now;
        }

        await _unitOfWork.CommitAsync(null, cancellationToken).ConfigureAwait(false);
    }
}
