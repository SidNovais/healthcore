using System;
using HC.Core.Domain;
using HC.LIS.Modules.UserAccess.Domain.Users.Events;
using HC.LIS.Modules.UserAccess.Domain.Users.Rules;

namespace HC.LIS.Modules.UserAccess.Domain.Users;

public class User : Entity, IAggregateRoot
{
    private UserId _id = null!;
    private UserEmail _email = null!;
    private string _fullName = string.Empty;
    private DateTime _birthdate;
    private string _gender = string.Empty;
    private UserRole _role = null!;
    private UserStatus _status = null!;
    private string? _invitationToken;
    private string? _passwordHash;
    private DateTime? _activatedAt;

    private User() { }

    public static User Create(
        Guid userId,
        string email,
        string fullName,
        DateTime birthdate,
        string gender,
        string role,
        string invitationToken,
        DateTime createdAt,
        Guid createdById)
    {
        User user = new();
        user._id = new UserId(userId);
        user._email = UserEmail.Of(email);
        user._fullName = fullName;
        user._birthdate = birthdate;
        user._gender = gender;
        user._role = UserRole.Of(role);
        user._status = UserStatus.PendingActivation;
        user._invitationToken = invitationToken;

        UserCreatedDomainEvent ev = new(
            userId, email, fullName, birthdate, gender, role,
            invitationToken, createdAt, createdById);
        user.AddEvent(ev);
        return user;
    }

    public void Activate(string invitationToken, string passwordHash, DateTime activatedAt)
    {
        CheckRule(new CannotActivateAlreadyActiveUserRule(_status));
        CheckRule(new CannotActivateWithInvalidTokenRule(_invitationToken!, invitationToken));

        _status = UserStatus.Active;
        _invitationToken = null;
        _passwordHash = passwordHash;
        _activatedAt = activatedAt;

        AddEvent(new UserActivatedDomainEvent(_id.Value, activatedAt));
    }

    public void ChangeRole(string newRole, Guid changedById, DateTime changedAt)
    {
        CheckRule(new CannotChangeRoleOfPendingUserRule(_status));

        string oldRole = _role.Value;
        _role = UserRole.Of(newRole);

        AddEvent(new UserRoleChangedDomainEvent(_id.Value, oldRole, newRole, changedById, changedAt));
    }
}
