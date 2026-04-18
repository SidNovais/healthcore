using HC.Core.Application.Events;
using HC.LIS.Modules.UserAccess.Domain.Users.Events;
using Newtonsoft.Json;

namespace HC.LIS.Modules.UserAccess.Application.Users.ActivateUser;

[method: JsonConstructor]
public class UserActivatedNotification(UserActivatedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<UserActivatedDomainEvent>(domainEvent, id)
{
}
