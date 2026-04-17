using System;
using FluentAssertions;
using HC.LIS.Modules.UserAccess.Domain.Users;
using HC.LIS.Modules.UserAccess.Domain.Users.Events;

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
}
