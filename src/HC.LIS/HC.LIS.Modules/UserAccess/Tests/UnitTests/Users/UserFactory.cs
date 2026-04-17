using HC.LIS.Modules.UserAccess.Domain.Users;

namespace HC.LIS.Modules.UserAccess.UnitTests.Users;

internal static class UserFactory
{
    public static User Create() =>
        User.Create(
            UserSampleData.UserId,
            UserSampleData.Email,
            UserSampleData.FullName,
            UserSampleData.Birthdate,
            UserSampleData.Gender,
            UserSampleData.Role,
            UserSampleData.InvitationToken,
            UserSampleData.CreatedAt,
            UserSampleData.CreatedById);
}
