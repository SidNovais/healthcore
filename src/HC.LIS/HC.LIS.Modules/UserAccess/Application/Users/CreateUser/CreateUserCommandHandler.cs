using HC.LIS.Modules.UserAccess.Application.Configuration.Commands;
using HC.LIS.Modules.UserAccess.Domain.Users;

namespace HC.LIS.Modules.UserAccess.Application.Users.CreateUser;

internal class CreateUserCommandHandler(
    IUserRepository userRepository
) : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<Guid> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        User user = User.Create(
            command.UserId,
            command.Email,
            command.FullName,
            command.Birthdate,
            command.Gender,
            command.Role,
            command.InvitationToken,
            command.CreatedAt,
            command.CreatedById);

        await _userRepository.AddAsync(user, cancellationToken).ConfigureAwait(false);

        return command.UserId;
    }
}
