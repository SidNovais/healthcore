using HC.Core.Application.Events;
using HC.LIS.Modules.UserAccess.Domain.Users.Events;
using Newtonsoft.Json;

namespace HC.LIS.Modules.UserAccess.Application.Users.ChangeRole;

[method: JsonConstructor]
public class UserRoleChangedNotification(UserRoleChangedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<UserRoleChangedDomainEvent>(domainEvent, id)
{
}
