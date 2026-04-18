using HC.Core.Application;
using HC.LIS.Modules.UserAccess.Application.Configuration.Commands;
using HC.LIS.Modules.UserAccess.Application.Configuration.Queries;

namespace HC.LIS.Modules.UserAccess.Application.Users.Login;

internal class LoginCommandHandler(
    IQueryHandler<GetUserByEmailQuery, UserAuthDataDto?> getUserByEmail,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IAuditLogWriter auditLogWriter
) : ICommandHandler<LoginCommand, LoginResultDto>
{
    private readonly IQueryHandler<GetUserByEmailQuery, UserAuthDataDto?> _getUserByEmail = getUserByEmail;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
    private readonly IAuditLogWriter _auditLogWriter = auditLogWriter;

    public async Task<LoginResultDto> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        UserAuthDataDto? user = await _getUserByEmail
            .Handle(new GetUserByEmailQuery(command.Email), cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            await _auditLogWriter.WriteAsync(
                null, null, "LoginFailed", $"UnknownEmail={command.Email}"
            ).ConfigureAwait(false);
            throw new InvalidCommandException("Invalid credentials.");
        }

        if (user.PasswordHash is null || !_passwordHasher.VerifyHashedPassword(user.PasswordHash, command.Password))
        {
            await _auditLogWriter.WriteAsync(
                user.Id, null, "LoginFailed", "InvalidPassword"
            ).ConfigureAwait(false);
            throw new InvalidCommandException("Invalid credentials.");
        }

        string token = _jwtTokenService.GenerateToken(user.Id, user.Email, user.Role);

        await _auditLogWriter.WriteAsync(user.Id, null, "LoginSuccess", null).ConfigureAwait(false);

        return new LoginResultDto(token, user.Id, user.Email, user.Role);
    }
}
