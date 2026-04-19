using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HC.Core.Application;
using HC.Core.Domain;
using HC.LIS.Modules.UserAccess.Application.Users.ActivateUser;
using HC.LIS.Modules.UserAccess.Application.Users.ChangeRole;
using HC.LIS.Modules.UserAccess.Application.Users.CreateUser;
using HC.LIS.Modules.UserAccess.Application.Users.GetAuditLog;
using HC.LIS.Modules.UserAccess.Application.Users.GetUserDetails;
using HC.LIS.Modules.UserAccess.Application.Users.Login;

namespace HC.LIS.Modules.UserAccess.IntegrationTests.Users;

public class UserTests : TestBase
{
    public UserTests() : base(Guid.CreateVersion7()) { }

    [Fact]
    public async Task CreateUserIsSuccessful()
    {
        await UserAccessModule.ExecuteCommandAsync(new CreateUserCommand(
            UserSampleData.UserId,
            UserSampleData.Email,
            UserSampleData.FullName,
            UserSampleData.Birthdate,
            UserSampleData.Gender,
            UserSampleData.Role,
            UserSampleData.InvitationToken,
            SystemClock.Now,
            UserSampleData.CreatedById
        )).ConfigureAwait(true);

        UserDetailsDto? details = await GetEventually(
            new GetUserDetailsFromUserAccessProbe(UserSampleData.UserId, UserAccessModule),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Id.Should().Be(UserSampleData.UserId);
        details.Email.Should().Be(UserSampleData.Email);
        details.FullName.Should().Be(UserSampleData.FullName);
        details.Gender.Should().Be(UserSampleData.Gender);
        details.Role.Should().Be(UserSampleData.Role);
        details.Status.Should().Be("PendingActivation");
        details.ActivatedAt.Should().BeNull();
    }

    [Fact]
    public async Task ActivateUserIsSuccessful()
    {
        var factory = new UserFactory(UserAccessModule);
        await factory.CreateAsync().ConfigureAwait(true);

        await GetEventually(
            new GetUserDetailsFromUserAccessProbe(UserSampleData.UserId, UserAccessModule),
            15000
        ).ConfigureAwait(true);

        await UserAccessModule.ExecuteCommandAsync(
            new ActivateUserCommand(
                UserSampleData.UserId,
                UserSampleData.InvitationToken,
                UserSampleData.PasswordHash,
                SystemClock.Now
        )).ConfigureAwait(true);

        UserDetailsDto? details = await GetEventually(
            new GetUserDetailsFromUserAccessProbe(
                UserSampleData.UserId,
                UserAccessModule,
                dto => dto?.Status == "Active"),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Status.Should().Be("Active");
        details.ActivatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ChangeRoleIsSuccessful()
    {
        var factory = new UserFactory(UserAccessModule);
        await factory.CreateActivatedAsync().ConfigureAwait(true);

        await GetEventually(
            new GetUserDetailsFromUserAccessProbe(
                UserSampleData.UserId,
                UserAccessModule,
                dto => dto?.Status == "Active"),
            15000
        ).ConfigureAwait(true);

        await UserAccessModule.ExecuteCommandAsync(
            new ChangeRoleCommand(
                UserSampleData.UserId,
                UserSampleData.NewRole,
                UserSampleData.CreatedById,
                SystemClock.Now
        )).ConfigureAwait(true);

        UserDetailsDto? details = await GetEventually(
            new GetUserDetailsFromUserAccessProbe(
                UserSampleData.UserId,
                UserAccessModule,
                dto => dto?.Role == UserSampleData.NewRole),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Role.Should().Be(UserSampleData.NewRole);
    }

    [Fact]
    public async Task LoginIsSuccessful()
    {
        var factory = new UserFactory(UserAccessModule);
        await factory.CreateActivatedAsync().ConfigureAwait(true);

        await GetEventually(
            new GetUserDetailsFromUserAccessProbe(
                UserSampleData.UserId,
                UserAccessModule,
                dto => dto?.Status == "Active"),
            15000
        ).ConfigureAwait(true);

        LoginResultDto result = await UserAccessModule.ExecuteCommandAsync<LoginResultDto>(
            new LoginCommand(UserSampleData.Email, UserSampleData.Password)
        ).ConfigureAwait(true);

        result.Token.Should().NotBeNullOrEmpty();
        result.UserEmail.Should().Be(UserSampleData.Email);
        result.Role.Should().Be(UserSampleData.Role);
    }

    [Fact]
    public async Task LoginFailedWritesAuditEntry()
    {
        var factory = new UserFactory(UserAccessModule);
        await factory.CreateActivatedAsync().ConfigureAwait(true);

        await GetEventually(
            new GetUserDetailsFromUserAccessProbe(
                UserSampleData.UserId,
                UserAccessModule,
                dto => dto?.Status == "Active"),
            15000
        ).ConfigureAwait(true);

        Func<Task> act = () => UserAccessModule.ExecuteCommandAsync<LoginResultDto>(
            new LoginCommand(UserSampleData.Email, "wrong-password")
        );
        await act.Should().ThrowAsync<InvalidCommandException>().ConfigureAwait(true);

        IReadOnlyCollection<AuditLogEntryDto> entries = await UserAccessModule.ExecuteQueryAsync<IReadOnlyCollection<AuditLogEntryDto>>(
            new GetAuditLogQuery(userId: UserSampleData.UserId)
        ).ConfigureAwait(true);

        entries.Should().Contain(e => e.EventType == "LoginFailed");
    }
}
