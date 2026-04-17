using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.UserAccess.Domain.Users.Events;

public class UserActivatedDomainEvent(Guid userId, DateTime activatedAt) : DomainEvent
{
    public Guid UserId { get; } = userId;
    public DateTime ActivatedAt { get; } = activatedAt;
}
