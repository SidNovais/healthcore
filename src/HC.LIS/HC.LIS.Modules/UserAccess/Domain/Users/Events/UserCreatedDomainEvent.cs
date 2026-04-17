using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.UserAccess.Domain.Users.Events;

public class UserCreatedDomainEvent(
    Guid userId,
    string email,
    string fullName,
    DateTime birthdate,
    string gender,
    string role,
    string invitationToken,
    DateTime createdAt,
    Guid createdById) : DomainEvent
{
    public Guid UserId { get; } = userId;
    public string Email { get; } = email;
    public string FullName { get; } = fullName;
    public DateTime Birthdate { get; } = birthdate;
    public string Gender { get; } = gender;
    public string Role { get; } = role;
    public string InvitationToken { get; } = invitationToken;
    public DateTime CreatedAt { get; } = createdAt;
    public Guid CreatedById { get; } = createdById;
}
