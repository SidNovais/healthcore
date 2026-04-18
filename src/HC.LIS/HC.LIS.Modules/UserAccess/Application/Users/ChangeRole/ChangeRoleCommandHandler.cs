using HC.Core.Application;
using HC.LIS.Modules.UserAccess.Application.Configuration.Commands;
using HC.LIS.Modules.UserAccess.Domain.Users;

namespace HC.LIS.Modules.UserAccess.Application.Users.ChangeRole;

internal class ChangeRoleCommandHandler(
    IUserRepository userRepository
) : ICommandHandler<ChangeRoleCommand>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task Handle(ChangeRoleCommand command, CancellationToken cancellationToken)
    {
        User user = await _userRepository.GetByIdAsync(new UserId(command.UserId), cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidCommandException("User must exist to change role.");

        user.ChangeRole(command.NewRole, command.ChangedById, command.ChangedAt);
    }
}
