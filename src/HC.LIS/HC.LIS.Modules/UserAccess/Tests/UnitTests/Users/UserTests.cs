using System;
using FluentAssertions;
using HC.LIS.Modules.UserAccess.Domain.Users;
using HC.LIS.Modules.UserAccess.Domain.Users.Events;
using HC.LIS.Modules.UserAccess.Domain.Users.Rules;

namespace HC.LIS.Modules.UserAccess.UnitTests.Users;

public class UserTests : TestBase
{
    private readonly User _sut;

    public UserTests()
    {
        _sut = UserFactory.Create();
    }

    [Fact]
    public void CreateUserIsSuccessful()
    {
        UserCreatedDomainEvent ev = AssertPublishedDomainEvent<UserCreatedDomainEvent>(_sut);
        ev.UserId.Should().Be(UserSampleData.UserId);
        ev.Email.Should().Be(UserSampleData.Email);
        ev.FullName.Should().Be(UserSampleData.FullName);
        ev.Birthdate.Should().Be(UserSampleData.Birthdate);
        ev.Gender.Should().Be(UserSampleData.Gender);
        ev.Role.Should().Be(UserSampleData.Role);
        ev.InvitationToken.Should().Be(UserSampleData.InvitationToken);
        ev.CreatedAt.Should().Be(UserSampleData.CreatedAt);
        ev.CreatedById.Should().Be(UserSampleData.CreatedById);
    }

    [Fact]
    public void ActivateUserIsSuccessful()
    {
        _sut.Activate(UserSampleData.InvitationToken, UserSampleData.PasswordHash, UserSampleData.ActivatedAt);
        UserActivatedDomainEvent ev = AssertPublishedDomainEvent<UserActivatedDomainEvent>(_sut);
        ev.UserId.Should().Be(UserSampleData.UserId);
        ev.ActivatedAt.Should().Be(UserSampleData.ActivatedAt);
    }

    [Fact]
    public void ActivateThrowsWhenTokenIsInvalid()
    {
        void action() => _sut.Activate("wrong-token", UserSampleData.PasswordHash, UserSampleData.ActivatedAt);
        AssertBrokenRule<CannotActivateWithInvalidTokenRule>(action);
    }

    [Fact]
    public void ActivateThrowsWhenUserIsAlreadyActive()
    {
        _sut.Activate(UserSampleData.InvitationToken, UserSampleData.PasswordHash, UserSampleData.ActivatedAt);
        void action() => _sut.Activate(UserSampleData.InvitationToken, UserSampleData.PasswordHash, UserSampleData.ActivatedAt);
        AssertBrokenRule<CannotActivateAlreadyActiveUserRule>(action);
    }

    [Fact]
    public void ChangeRoleIsSuccessful()
    {
        _sut.Activate(UserSampleData.InvitationToken, UserSampleData.PasswordHash, UserSampleData.ActivatedAt);
        _sut.ChangeRole(UserSampleData.NewRole, UserSampleData.ChangedById, UserSampleData.ChangedAt);
        UserRoleChangedDomainEvent ev = AssertPublishedDomainEvent<UserRoleChangedDomainEvent>(_sut);
        ev.UserId.Should().Be(UserSampleData.UserId);
        ev.OldRole.Should().Be(UserSampleData.Role);
        ev.NewRole.Should().Be(UserSampleData.NewRole);
        ev.ChangedById.Should().Be(UserSampleData.ChangedById);
        ev.ChangedAt.Should().Be(UserSampleData.ChangedAt);
    }

    [Fact]
    public void ChangeRoleThrowsWhenUserIsPending()
    {
        void action() => _sut.ChangeRole(UserSampleData.NewRole, UserSampleData.ChangedById, UserSampleData.ChangedAt);
        AssertBrokenRule<CannotChangeRoleOfPendingUserRule>(action);
    }
}
