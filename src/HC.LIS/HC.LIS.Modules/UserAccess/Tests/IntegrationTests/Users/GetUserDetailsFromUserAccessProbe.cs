using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.UserAccess.Application.Contracts;
using HC.LIS.Modules.UserAccess.Application.Users.GetUserDetails;

namespace HC.LIS.Modules.UserAccess.IntegrationTests.Users;

public class GetUserDetailsFromUserAccessProbe(
    Guid expectedUserId,
    IUserAccessModule userAccessModule,
    Func<UserDetailsDto?, bool>? satisfiedWhen = null
) : IProbe<UserDetailsDto>
{
    private readonly Guid _expectedUserId = expectedUserId;
    private readonly IUserAccessModule _userAccessModule = userAccessModule;
    private readonly Func<UserDetailsDto?, bool> _satisfiedWhen = satisfiedWhen ?? (dto => dto is not null);

    public string DescribeFailureTo() =>
        $"UserDetails not found or unsatisfied for {_expectedUserId}";

    public async Task<UserDetailsDto?> GetSampleAsync()
    {
        return await _userAccessModule
            .ExecuteQueryAsync(new GetUserDetailsQuery(_expectedUserId))
            .ConfigureAwait(false);
    }

    public bool IsSatisfied(UserDetailsDto? sample) => _satisfiedWhen(sample);
}
