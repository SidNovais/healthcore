using HC.LIS.Modules.UserAccess.Application.Contracts;

namespace HC.LIS.Modules.UserAccess.Application.Users.ChangeRole;

public class ChangeRoleCommand(
    Guid userId,
    string newRole,
    Guid changedById,
    DateTime changedAt
) : CommandBase
{
    public Guid UserId { get; } = userId;
    public string NewRole { get; } = newRole;
    public Guid ChangedById { get; } = changedById;
    public DateTime ChangedAt { get; } = changedAt;
}
