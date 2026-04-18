using HC.Core.Application;
using HC.LIS.Modules.UserAccess.Application.Configuration.Commands;
using HC.LIS.Modules.UserAccess.Domain.Users;

namespace HC.LIS.Modules.UserAccess.Application.Users.ActivateUser;

internal class ActivateUserCommandHandler(
    IUserRepository userRepository
) : ICommandHandler<ActivateUserCommand>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task Handle(ActivateUserCommand command, CancellationToken cancellationToken)
    {
        User user = await _userRepository.GetByIdAsync(new UserId(command.UserId), cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidCommandException("User must exist to activate.");

        user.Activate(command.InvitationToken, command.PasswordHash, command.ActivatedAt);
    }
}
