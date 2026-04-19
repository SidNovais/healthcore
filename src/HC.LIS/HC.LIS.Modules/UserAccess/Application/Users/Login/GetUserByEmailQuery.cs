using HC.LIS.Modules.UserAccess.Application.Contracts;

namespace HC.LIS.Modules.UserAccess.Application.Users.Login;

internal class GetUserByEmailQuery(string email) : QueryBase<UserAuthDataDto?>
{
    public string Email { get; } = email;
}
