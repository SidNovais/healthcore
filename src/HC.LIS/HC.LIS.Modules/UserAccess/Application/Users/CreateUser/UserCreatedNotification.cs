using HC.Core.Application.Events;
using HC.LIS.Modules.UserAccess.Domain.Users.Events;
using Newtonsoft.Json;

namespace HC.LIS.Modules.UserAccess.Application.Users.CreateUser;

[method: JsonConstructor]
public class UserCreatedNotification(UserCreatedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<UserCreatedDomainEvent>(domainEvent, id)
{
}
