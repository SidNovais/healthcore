using MediatR;

namespace HC.LIS.Modules.UserAccess.Application.Users.ChangeRole;

public class UserRoleChangedNotificationHandler(IAuditLogWriter auditLogWriter)
    : INotificationHandler<UserRoleChangedNotification>
{
    private readonly IAuditLogWriter _auditLogWriter = auditLogWriter;

    public async Task Handle(UserRoleChangedNotification notification, CancellationToken cancellationToken)
    {
        await _auditLogWriter.WriteAsync(
            notification.DomainEvent.UserId,
            notification.DomainEvent.ChangedById,
            "RoleChanged",
            $"OldRole={notification.DomainEvent.OldRole},NewRole={notification.DomainEvent.NewRole}"
        ).ConfigureAwait(false);
    }
}
