using HC.LIS.Modules.UserAccess.Application.Contracts;

namespace HC.LIS.Modules.UserAccess.Application.Users.ActivateUser;

public class ActivateUserCommand(
    Guid userId,
    string invitationToken,
    string passwordHash,
    DateTime activatedAt
) : CommandBase
{
    public Guid UserId { get; } = userId;
    public string InvitationToken { get; } = invitationToken;
    public string PasswordHash { get; } = passwordHash;
    public DateTime ActivatedAt { get; } = activatedAt;
}
