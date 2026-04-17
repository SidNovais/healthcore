using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.UserAccess.Domain.Users.Events;

public class UserRoleChangedDomainEvent(
    Guid userId,
    string oldRole,
    string newRole,
    Guid changedById,
    DateTime changedAt) : DomainEvent
{
    public Guid UserId { get; } = userId;
    public string OldRole { get; } = oldRole;
    public string NewRole { get; } = newRole;
    public Guid ChangedById { get; } = changedById;
    public DateTime ChangedAt { get; } = changedAt;
}
