using System.Threading.Tasks;
using HC.Core.Domain;
using HC.LIS.Modules.UserAccess.Application.Contracts;
using HC.LIS.Modules.UserAccess.Application.Users.ActivateUser;
using HC.LIS.Modules.UserAccess.Application.Users.CreateUser;

namespace HC.LIS.Modules.UserAccess.IntegrationTests.Users;

public class UserFactory(IUserAccessModule userAccessModule)
{
    private readonly IUserAccessModule _userAccessModule = userAccessModule;

    public async Task CreateAsync()
    {
        await _userAccessModule.ExecuteCommandAsync(new CreateUserCommand(
            UserSampleData.UserId,
            UserSampleData.Email,
            UserSampleData.FullName,
            UserSampleData.Birthdate,
            UserSampleData.Gender,
            UserSampleData.Role,
            UserSampleData.InvitationToken,
            SystemClock.Now,
            UserSampleData.CreatedById
        )).ConfigureAwait(false);
    }

    public async Task CreateActivatedAsync()
    {
        await CreateAsync().ConfigureAwait(false);
        await _userAccessModule.ExecuteCommandAsync(new ActivateUserCommand(
            UserSampleData.UserId,
            UserSampleData.InvitationToken,
            UserSampleData.PasswordHash,
            SystemClock.Now
        )).ConfigureAwait(false);
    }
}
