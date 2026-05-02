using HC.Core.Application;
using HC.LIS.Modules.UserAccess.Application.Configuration.Commands;
using HC.LIS.Modules.UserAccess.Application.Users;
using HC.LIS.Modules.UserAccess.Domain.Users;

namespace HC.LIS.Modules.UserAccess.Application.Users.ActivateUser;

internal class ActivateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher
) : ICommandHandler<ActivateUserCommand>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    public async Task Handle(ActivateUserCommand command, CancellationToken cancellationToken)
    {
        User user = await _userRepository.GetByIdAsync(new UserId(command.UserId), cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidCommandException("User must exist to activate.");

        string passwordHash = _passwordHasher.HashPassword(command.Password);
        user.Activate(command.InvitationToken, passwordHash, command.ActivatedAt);
    }
}
